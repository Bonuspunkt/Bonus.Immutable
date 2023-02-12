use C#9 Records

# Bonus.Immutable
[![Build status](https://ci.appveyor.com/api/projects/status/it69oo1vy2a6ix00/branch/master?svg=true)](https://ci.appveyor.com/project/Bonuspunkt/bonus-immutable/branch/master)
[![Nuget version](https://img.shields.io/nuget/v/Bonus.Immutable.svg)](https://www.nuget.org/packages/Bonus.Immutable/)


Immutable DataTypes for .net

## sample usage
``` C#
// "record" declaration
public interface IObject : IImmutable<IObject> {
    int Number { get; }
    string Text { get; }
}

// creating an instance
var resolver = TypeGenerator.Generate(new[]{ typeof(IObject) });
var instance = resolver.CreateInstance<IObject>();

// changing properties
var newInstance = instance
    .Set(i => i.Number, 1)
    .Set(i => i.Text, "string");
```
