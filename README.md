# Micro IOC
A No Frills IOC
Build: ![BuildState](https://travis-ci.org/rincewound/MicroIOC.svg?branch=master)

##Introduction
Why another IOC? Easy: I needed something simple for unittesting and had no
internet access, so I whipped something up myself. Turned out to be rather
usable, so I decided to share.

##Using it...
* To register a type use MicroIOC.IOC.Register<Type>(<GeneratorFunction>). You can use any function, lamba or similar as a generator, as long as it has the correct return type (i.e. the type used for the generic). Note that providing the wrong type is not caught at compiletime at this point, but at runtime, when the type is first resolved.
* To resolve a type use MicroIOC.IOC.Resolve<Type>();
* Resolve will attempt to recursively resolve dependencies as well, if they are marked with the "MuImport" Attribute.

##Example

```C#

interface Greeter
{
  void Greet();
}

class GreeterImpl: Greeter
{
  public void Greet()
  {
    System.Console.WriteLine("Hello World!");
  }
}

class Importer
{
  [MuImport]
  public Greeter myGreeter;
}

public static void main()
{
  IOC.Register<Greeter>(() => {return new GreeterImpl();});
  IOC.Register<Importer>(() => {return new Importer();});

  var theGreeter = IOC.Resolve<Greeter>();

  theGreeter.Greet(); // Prints "Hello World!"

  var imp = IOC.Resolve<Importer>();

  imp.myGreeter.Greet();  // Prints "Hello World!"
}
```

##ToDo
* MicroIOC will not yet resolve private properties.
* When resolving a dependency, the first available constructor will be used.
