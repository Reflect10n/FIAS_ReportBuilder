using Microsoft.EntityFrameworkCore;

namespace WebApplication6.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Operations.Any())
                {
                    return;
                }
                context.Operations.AddRange(
                    new Operation
                    {
                        Id = 1,
                        Action = "инициация",
                    },
                    new Operation
                    {
                        Id = 10,
                        Action = "добавление",
                    },
                    new Operation
                    {
                        Id = 20,
                        Action = "изменение",
                    },
                    new Operation
                    {
                        Id = 21,
                        Action = "групповое изменение",
                    },
                    new Operation
                    {
                        Id = 30,
                        Action = "удаление",
                    },
                    new Operation
                    {
                        Id = 31,
                        Action = "удаление вследствие вышестоящего объекта",
                    },
                    new Operation
                    {
                        Id = 40,
                        Action = "присоединение адресного объекта (слияние)",
                    },
                    new Operation
                    {
                        Id = 41,
                        Action = "переподчинение вследствие слияния вышестоящего объекта",
                    },
                    new Operation
                    {
                        Id = 42,
                        Action = "прекращение существования вследствие присоединения к другому адресному объекту",
                    },
                    new Operation
                    {
                        Id = 43,
                        Action = "создание нового адресного объекта в результате слияния адресных объектов",
                    },
                    new Operation
                    {
                        Id = 50,
                        Action = "переподчинение",
                    },
                    new Operation
                    {
                        Id = 51,
                        Action = "переподчинение вследствие переподчинения вышестоящего объекта",
                    },
                    new Operation
                    {
                        Id = 60,
                        Action = "прекращение существования вследствие дробления",
                    },
                    new Operation
                    {
                        Id = 61,
                        Action = "создание нового адресного объекта в результате дробления",
                    },
                    new Operation
                    {
                        Id = 70,
                        Action = "восстановление прекратившего существование объекта",
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
