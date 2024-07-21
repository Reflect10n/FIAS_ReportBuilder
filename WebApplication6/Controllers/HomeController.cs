using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateDB()
        {
            return View();
        }

        public async Task<IActionResult> ViewReport()
        {
            var updateDateEntry = await _context.UpdateDates.FirstOrDefaultAsync();
            var updateDate = updateDateEntry?.Date ?? DateTime.MinValue;
            ViewData["UpdateDate"] = updateDate.ToString("dd.MM.yyyy");

            var objectLevels = await _context.ObjectLevels
                .OrderBy(ol => ol.Level)
                .ToListAsync();

            var reportData = new List<LevelReport>();
            foreach (var objectLevel in objectLevels)
            {
                var objectAsAddrs = await _context.ObjectAsAddrs
                    .Include(o => o.Operation)
                    .Where(objAsAddr => objAsAddr.Level == objectLevel.Level)
                    .OrderBy(objAsAddr => objAsAddr.Name)
                    .ToListAsync();

                if (objectAsAddrs.Any())
                {
                    var levelReport = new LevelReport
                    {
                        Level = objectLevel.Level,
                        Name = objectLevel.Name,
                        Objects = objectAsAddrs.Select(objAsAddr => new ObjectDetail
                        {
                            Id = objAsAddr.Id,
                            GuID = objAsAddr.GuID,
                            TypeName = objAsAddr.Typename,
                            Name = objAsAddr.Name,
                            ActionDescription = objAsAddr.Operation.Action,
                        }).ToList()
                    };
                    reportData.Add(levelReport);
                }
            }
            return View(reportData);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAndAnalyzeZip()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не загружен.");
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            else
            {
                DeleteFilesInDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractPath = Path.Combine(uploadPath, Path.GetFileNameWithoutExtension(file.FileName));
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            ZipFile.ExtractToDirectory(filePath, extractPath);

            var versionFilePath = Path.Combine(extractPath, "version.txt");
            if (System.IO.File.Exists(versionFilePath))
            {
                var lines = await System.IO.File.ReadAllLinesAsync(versionFilePath);
                if (lines.Length > 0)
                {
                    if (DateTime.TryParseExact(lines[0], "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out var updateDate))
                    {
                        await _context.Database.ExecuteSqlRawAsync("DELETE FROM UpdateDates");
                        var sql = "INSERT INTO UpdateDates (Date) VALUES (@p0)";
                        await _context.Database.ExecuteSqlRawAsync(sql, updateDate);
                    }
                }
            }
            await AddToObjectLevels(extractPath);
            await AddToObjectAsAddr(extractPath);
            
            DeleteFilesInDirectory(uploadPath);

            return Ok("Обновление БД завершено.");
        }

        private void DeleteFilesInDirectory(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при удалении файла: {file}");
                }
            }

            var directories = Directory.GetDirectories(directoryPath);
            foreach (var directory in directories)
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при удалении директории: {directory}");
                }
            }
        }
        
        private async Task AddToObjectLevels(string extractPath)
        {
            await DeleteAllRecordsFromObjectLevelsAsync();
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ObjectLevels', RESEED, 0);");

            var pattern = @"^AS_OBJECT_LEVELS_\d+.*\.xml$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var files = Directory.GetFiles(extractPath, "*.xml", SearchOption.TopDirectoryOnly)
                .Where(file => regex.IsMatch(Path.GetFileName(file)))
                .ToList();

            foreach (var file in files)
            {
                var doc = XDocument.Load(file);

                if (!IsValidXmlStructureLevel(doc))
                {
                    _logger.LogWarning($"Файл имеет некорректную структуру XML: {file}");
                    continue;
                }

                var objects = doc.Descendants("OBJECTLEVEL")
                    .Select(o => new ObjectLevel
                    {
                        Level = (int)o.Attribute("LEVEL"),
                        Name = (string)o.Attribute("NAME"),
                        IsActive = (bool)o.Attribute("ISACTIVE"),
                    })
                    .ToList();

                await _context.ObjectLevels.AddRangeAsync(objects);
            }
            await _context.SaveChangesAsync();
        }
        private async Task AddToObjectAsAddr(string extractPath)
        {
            await DeleteAllRecordsFromObjectAsAddrsAsync();

            var pattern = @"^AS_ADDR_OBJ_\d+.*\.xml$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var files = Directory.GetFiles(extractPath, "*.xml", SearchOption.AllDirectories)
                .Where(file => regex.IsMatch(Path.GetFileName(file)))
                .ToList() ;
            foreach (var file in files)
            {
                var doc = XDocument.Load(file);

                if (!IsValidXmlStructureAddr(doc))
                {
                    _logger.LogWarning($"Файл имеет некорректную структуру XML: {file}");
                    continue;
                }

                var objects = doc.Descendants("OBJECT")
                    .Select(o =>
                    {
                        var isActive = parseNullableBool((string)o.Attribute("ISACTIVE"));
                        if (isActive.HasValue && isActive.Value == true)
                        {
                            return new ObjectAsAddr
                            {
                                Id = (int)o.Attribute("ID"),
                                GuID = (string)o.Attribute("OBJECTGUID"),
                                Name = (string)o.Attribute("NAME"),
                                Typename = (string)o.Attribute("TYPENAME"),
                                Level = (int)o.Attribute("LEVEL"),
                                OperType_Id = (int)o.Attribute("OPERTYPEID"),
                                IsActive = isActive.Value,
                            };
                        }
                        return null;
                    })
                    .Where(o => o != null)
                    .ToList();

                await _context.ObjectAsAddrs.AddRangeAsync(objects);
            }

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllRecordsFromObjectAsAddrsAsync()
        {
            _context.ObjectAsAddrs.RemoveRange(_context.ObjectAsAddrs);
                await _context.SaveChangesAsync();
        }

        private async Task DeleteAllRecordsFromObjectLevelsAsync()
        {
            _context.ObjectLevels.RemoveRange(_context.ObjectLevels);
            await _context.SaveChangesAsync();
        }

        private bool? parseNullableBool(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value == "1";
        }
        private bool IsValidXmlStructureAddr(XDocument doc)
        {
            var root = doc.Root;
            return root != null && root.Name.LocalName == "ADDRESSOBJECTS" &&
                   root.Descendants("OBJECT").Any();
        }

        private bool IsValidXmlStructureLevel(XDocument doc)
        {
            var root = doc.Root;
            return root != null && root.Name.LocalName == "OBJECTLEVELS" &&
                   root.Descendants("OBJECTLEVEL").Any();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
