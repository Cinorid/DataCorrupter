# Corrupter
Corrupt any file with random data.

``` dotnet tool install -g Cinorid.corrupter ```

and then use it:

```
Description:
  Corrupt any file with random data.

Usage:
  corrupter [options]

Options:
  -f, --file <inputFile> (REQUIRED)  Input file name or path
  -o, --out <outputFile>             Output file name or path
  -r, --ratio <ratioNumber>          insert 1 random bytes per N bytes [default: 1000]
  -p, --inplace                      [WARNING] makes all changes into input [default: False]
  --version                          Show version information
  -?, -h, --help                     Show help and usage information
```
