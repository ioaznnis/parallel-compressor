# parallel-compressor

Консольное приложение на C# для поблочного сжатия и распаковки файлов с помощью System.IO.Compression.GzipStream.

Для сжатия исходный файл делится на блоки одинакового размера (константа `BufferSize`). 
Каждый блок сжимается и записывается в выходной файл независимо от остальных блоков.

Программа эффективно распараллеливает и синхронизирует обработку блоков в многопроцессорной среде и умеет обрабатывать файлы, размер которых превышает объем доступной оперативной памяти.

При работе с потоками используются только базовые классы и объекты синхронизации:
* Thread
* Manual/AutoResetEvent
* Monitor, и оператор lock.
* SemaphoreSlim

## ToDo

* Сохранения порядка блоков
* Обработку исключений
* Распаковку файлов
* Инициализацию из командной строки в формате:  
`GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла]`
* Корректное завершение работы программы: В случае успеха программа должна возвращать 0, при ошибке возвращать 1.
* Unit тесты

> Примечание: формат архива приближен к формату GZIP, т.е. блоки GZIP записываются последовательно. Такой формат удается разархивировать, используя WinRar, однако он не позволяет реализовать эффективную параллельную распаковку, так как размер блока неизвестен. И нет гарантии, что очередной блок поместится в оперативную память.
