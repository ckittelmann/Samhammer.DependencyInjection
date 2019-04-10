﻿using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Samhammer.DependencyInjection.Test.TestData.FactoryClass;
using Samhammer.DependencyInjection.Test.TestData.InjectedClass;
using Samhammer.DependencyInjection.Test.TestData.InjectedList;
using Xunit;

namespace Samhammer.DependencyInjection.Test
{
    public class DependencyResolverTest
    {
        private readonly IServiceCollection serviceCollection;

        private readonly ILogger<DependencyResolver> logger;

        private ServiceProvider serviceProvider;

        public DependencyResolverTest()
        {
            logger = Substitute.For<ILogger<DependencyResolver>>();
            serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(logger);
        }

        [Fact]
        private void GetClass_WithDefaultLifetime_Scoped()
        {
            // act
            serviceCollection.ResolveDependencies();
            serviceProvider = serviceCollection.BuildServiceProvider();

            IClassDefaultLifetime service;
            IClassDefaultLifetime service2;
            IClassDefaultLifetime service3;

            using (var scope = serviceProvider.CreateScope())
            {
                service = scope.ServiceProvider.GetService<IClassDefaultLifetime>();
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                service2 = scope2.ServiceProvider.GetService<IClassDefaultLifetime>();
                service3 = scope2.ServiceProvider.GetService<IClassDefaultLifetime>();
            }

            // assert
            service.Should().NotBeNull().And.BeOfType<ClassDefaultLifetime>();
            service2.Should().NotBe(service);
            service3.Should().Be(service2);
        }

        [Fact]
        private void GetClass_WithInheritedLifetime_Singleton()
        {
            // act
            serviceCollection.ResolveDependencies();
            serviceProvider = serviceCollection.BuildServiceProvider();

            IClassInheritedLifetimeSingleton service = serviceProvider.GetService<IClassInheritedLifetimeSingleton>();
            IClassInheritedLifetimeSingleton service2;
            IClassInheritedLifetimeSingleton service3;

            using (var scope = serviceProvider.CreateScope())
            {
                service2 = scope.ServiceProvider.GetService<IClassInheritedLifetimeSingleton>();
                service3 = scope.ServiceProvider.GetService<IClassInheritedLifetimeSingleton>();
            }

            // assert
            service.Should().NotBeNull().And.BeOfType<ClassInheritedLifetimeSingleton>();
            service2.Should().Be(service);
            service3.Should().Be(service2);
        }

        [Fact]
        private void GetClass_WithOverridenLifetime_Transient()
        {
            // act
            serviceCollection.ResolveDependencies();
            serviceProvider = serviceCollection.BuildServiceProvider();

            IClassOverridenLifetimeTransient service = serviceProvider.GetService<IClassOverridenLifetimeTransient>();
            IClassOverridenLifetimeTransient service2;
            IClassOverridenLifetimeTransient service3;

            using (var scope = serviceProvider.CreateScope())
            {
                service2 = scope.ServiceProvider.GetService<IClassOverridenLifetimeTransient>();
                service3 = scope.ServiceProvider.GetService<IClassOverridenLifetimeTransient>();
            }

            // assert
            service.Should().NotBeNull().And.BeOfType<ClassOverridenLifetimeTransient>();
            service2.Should().NotBe(service);
            service3.Should().NotBe(service2);
        }

        [Fact]
        public void GetClass_WithInjectedList()
        {
            // act
            serviceCollection.ResolveDependencies();
            serviceProvider = serviceCollection.BuildServiceProvider();
            var targetService = (TargetService)serviceProvider.GetService<ITargetService>();

            // assert
            targetService.Should().NotBeNull().And.BeOfType<TargetService>();
            targetService.Services.Should().HaveCount(3);

            Assert.Contains(targetService.Services, s => s.GetType() == typeof(Service1));
            Assert.Contains(targetService.Services, s => s.GetType() == typeof(Service2));
            Assert.Contains(targetService.Services, s => s.GetType() == typeof(ServiceInherited));
        }

        [Fact]
        private void GetClass_FromFactory()
        {
            // act
            serviceCollection.ResolveDependencies();
            serviceProvider = serviceCollection.BuildServiceProvider();

            IClassFromFactory service = serviceProvider.GetService<IClassFromFactory>();

            // assert
            service.Should().NotBeNull().And.BeOfType<ClassFromFactory>();
        }

        [Fact]
        private void GetFactoryMethods_FromFactory()
        {
            // arrange
            var resolver = new DependencyResolver(logger);

            // act
            var methods = resolver.GetFactoryMethods(typeof(Factory));

            // assert
            methods.Should().HaveCount(1);
            methods.First().Should().Match(x => x.Name.Equals(nameof(Factory.Build)));
        }
    }
}
