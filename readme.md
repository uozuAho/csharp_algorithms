# Algorithms, in C#

Most of this is from https://algs4.cs.princeton.edu. Assume all code here is
owned by them. This project is just for my interest/reference.

# Usage
Install dotnet 8+

```sh
dotnet test   # build & run tests

# Run an algorithm by class name, streaming in test data.
# All algorithms have a main method that reads from stdin.
cd algs.console
curl https://algs4.cs.princeton.edu/15uf/tinyUF.txt | dotnet run quickfinduf

# benchmarks
cd algs.bench
dotnet run quickfinduf_find                      # run fast approx benchmarks
dotnet run -c Release quickfinduf_find --bnet    # run accurate benchmarks using benchmarkdotnet
```

# todo
- WIP: tries https://algs4.cs.princeton.edu/52trie/
