Небольшое веб-приложение для формирования отчета по изменениям в адресных объектах, внесенных ФИАС
Для формирования отчета требуется заполнить базу данных. Она заполняется посредством парсинга различных файлов из .zip архива с изменениями с сайта (в приложении есть кнопка с ссылкой на скачивание этих файлов).
После успешного парсинга можно формировать отчет сколько угодно раз, без необходимости парсить файлы из архива.
При обновлении данных в БД (т.е. например при загрузке нового архива), старые данные удаляются.
Для пользования программой надо указать в файле appsettings.json в строке "DefaultConnection" путь до базы данных, она есть в главном каталоге программы. Чтобы указать её нужно дважды щелкнуть по DbTest2.mdf в обозревателе решений и скопировать данные из "Строка подключений" в "DefaultConnection"
Видео-демонстрация работы программы: 