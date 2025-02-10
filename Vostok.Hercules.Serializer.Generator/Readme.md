# Vostok.Hercules.Serializer

Source generator for emitting EventReaders for Vostok.Hercules protocol. 
Helps you to implement most of the code in automatic way via annotations:
- Put `[GenerateHerculesReader]` annotation on your type
- Then specify source keys for properties with `[HerculesTag("key1")]` annotations
- _optional_ Specify converter with `[HerculesConverter]` annotation
- ???
- Enjoy generated HerculesEventReader for your type

### Wtf is source generators?!

Here some useful links for quick dive-in into Source Generators:
- [basic concepts](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [details and examples](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)

### Some comments about structure

At current version SourceGenerators doesn't provide the API to emit the code 
(and it is the [official position](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#use-an-indented-text-writer-not-syntaxnodes-for-generation)). 
Roslyn developers just suggest you to use your own writer based on StringBuilder.

To avoid low-level details (such as styling or taking care about brackets and so on) there is two important abstractions in the [./Core] folder:
- High-level builders (such as TypeBuilder or MethodBuilder) are located in [./Core/Builders] folder. 
  Their primary goal to give abstraction for emitting metadata constructs. 
  Actually by design they act just like anemic models and only contain some data for future processing.
- Actual writer is located in [./Core/Writer] folder. It contains a very few amount of methods and designed to be extended via extensions-method.
- Extensions are divided in two major groups: Append and Write with corresponding prefixes. 
  - Append... extensions act just like a regular `StringBuilder.Append..` methods: only appending text with some formatting without any rich logic
  - Write... extensions are more high-level and contain rich logic: spacing, indentation, writing syntax construct and so one

Also, there is should be a set of extensions for methods emitting, 
but executable code (such as method bodies) is way more complicated topic, 
and it's really easier to emit it just via `Append(string)`

This design is kinda chaotic, but it allows to not care about low level details while writing actual business-logic with a reasonable cost.


## TODO

- Add generated builder configuration (re-use builderProvider, nullability, implicit type conversions, etc)
- Move API to own NuGet package
- Approval tests (separated by generated code and diagnostic messages)
- Refactor HerculesConverterEmitter to reuse same code concepts and get rid of magic strings (Expressions-like tree?)
- More strict validations on attributes
- Add API to deserialize containers not only from types marked with GenerateHerculesReaderAttribute