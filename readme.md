# Algorithms, in C#

Most of this is from https://algs4.cs.princeton.edu. Assume all code here is
owned by them. This project is just for my interest/reference.

# Usage
Install dotnet 8+

```sh
dotnet build      # does it build?
# todo: tests

# Run an algorithm by class name, streaming in test data.
# All algorithms have a main method that reads from stdin.
cd algs.console
curl https://algs4.cs.princeton.edu/15uf/tinyUF.txt | dotnet run quickfinduf

# benchmark an algorithm by benchmark class name
cd algs.bench
dotnet run -c Release quickfinduf_find
```
