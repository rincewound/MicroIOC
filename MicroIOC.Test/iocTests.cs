using System;

using MicroIOC;
using NUnit.Framework;

namespace IOCTest
{
    [TestFixture]
    public class iocTests
    {
        public interface IFoo { int A(); }

        public class FooImpl : IFoo
        {
            public int A() { return 2; }
        }

        public interface IFnord { string B(); }
        public class FnordImpl : IFnord { public string B() { return "b"; } }

        public class Importeur
        {
            [MuImport]
            public IFnord importedFnord;

            public IFnord notImportedFnord;
        }

        public class BadImport
        {
            [MuImport]
            public string importedString;
        }

        [SetUp]
        public void Setup()
        {
            IOC.Reset();
            IOC.Register<IFnord>(() => { return new FnordImpl(); });
        }

        [Test]
        public void Resolve_ReturnsCorrectInstance()
        {
            IOC.Register<IFoo>(() => { return new FooImpl(); });
            var theFoo = IOC.Resolve<IFoo>();
            Assert.IsTrue(theFoo is FooImpl);
        }

        [Test]
        public void Resolve_BadType_ThrowsException()
        {
            try
            {
                var fnord = IOC.Resolve<string>();
                Assert.Fail();  // should never get here!
            }
            catch (UnknownTypeException)
            {
                // All good, expected exception thrown.
            }
        }

        [Test]
        public void ImportAttribute_ResolvedAutomatically()
        {
            var imp = new Importeur();
            IOC.ResolveImports(imp);
            Assert.IsNotNull(imp.importedFnord);
            Assert.IsTrue(imp.importedFnord is FnordImpl);
            Assert.IsNull(imp.notImportedFnord);
        }

        [Test]
        public void ImportAttribute_ResolveThrowsException_If_TypeUnknown()
        {
            try
            {
                var fnord = new BadImport();
                IOC.ResolveImports(fnord);
                Assert.Fail();  // should never get here!
            }
            catch (UnknownTypeException)
            {
                // All good!
            }
        }

        interface IRecursiveTest
        {
            void A();
        }

        class RecImplA: IRecursiveTest
        {
            public void A() { }
        }

        class RecImportA
        {
            [MuImport]
            public IRecursiveTest tstA;

        }

        class RecImportB
        {
            [MuImport]
            public RecImportA impA;
        }

        [Test]
        public void ResolveImports_ResolvesRecursively()
        {
            IOC.Register<RecImportB>(() => { return new RecImportB(); });
            IOC.Register<RecImportA>(() => { return new RecImportA(); });
            IOC.Register<IRecursiveTest>(() => { return new RecImplA(); });

            RecImportB theB = IOC.Resolve<RecImportB>();

            Assert.IsNotNull(theB.impA);
            Assert.IsNotNull(theB.impA.tstA);

        }
    }
}
