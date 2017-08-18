# Hake.Extension.DependencyInjection
DependencyInjection using dotnet core

By default, almost all prgoramming languages allow developers to invoke methods by passing exactly matched parameters. This is a most nature way as our first class told us you have to do so or you cannot make function calls. While in some cases this could be fussy that you have no way to make calls to different functions in an unified way.

Also, while rebuilding your codes, any change of function signture could cause some repeated work that change every line of code which calls this function.

Then dependency injection allows target methods to be invoked by matching parameters automatically. The only thing developers have to do is passing the necessary informations about target methods and a set of values which is used to match parameters to some `Factory` classes, and they could complete the remaining works.

## Services
Methods require parameters as their input. Those instances passed as parameters are called `services`. We use `ServiceDescriptor` to present services.

There are three ways to get corresponding service from a specific `ServiceDescriptor`
- By returning predefined instance
- Let dependency injection calls the constructor of required type
- Invoke a factory method to create instance

## Service Lifetime
Lifetime of `ServiceDescriptor` defines when should service instances should be re-created.
- Singleton
    
    instances should only be created once within the whole application lifecycle.

- Scoped

    instances should be re-created when entering a new scope. There are many definitions of 'scope'.

- Transient

    instances should be created every time they are required.

## Service Pool
We build a service pool to hold configured services. Before calling to any method, every instance of sepecific parameter type from the method signature is grabbed from the pool.

## Matching Parameters
`Dictionary<string, object>` a dictionary is used to match parameters by name and type.

`params object[]` these optional extra parameters which passed when making dependency injection calls are used to match parameters by type sequentially.

`IServiceProvider` service pool is used to find corresponding services to match parameters by declared type in `ServiceDescriptor`.

**Matching sequence:**
1. from dictionary
2. match value(s) to list/array from dictionary if possible
3. from array
4. from services
5. raise event and try get value
6. use parameter default value
7. match value(s) to list/array from array if possible
8. use default value of parameter type

## Code
By referring to test codes, you can see how the dependency injection calls methods and creates objects.