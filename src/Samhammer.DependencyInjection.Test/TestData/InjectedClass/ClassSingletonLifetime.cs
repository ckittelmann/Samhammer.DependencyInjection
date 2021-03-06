﻿using Microsoft.Extensions.DependencyInjection;
using Samhammer.DependencyInjection.Attributes;

namespace Samhammer.DependencyInjection.Test.TestData.InjectedClass
{
    [Inject(lifetime: ServiceLifetime.Singleton)]
    public class ClassSingletonLifetime : IClassSingletonLifetime
    {
    }

    public interface IClassSingletonLifetime
    {
    }
}
