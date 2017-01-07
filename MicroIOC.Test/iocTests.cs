using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroIOC;

namespace IOCTest
{
    [TestClass]
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

        public iocTests()
        {
            IOC.Reset();
            IOC.Register<IFnord>(() => { return new FnordImpl(); });
        }

        [TestMethod]
        public void Resolve_ReturnsCorrectInstance()
        {
            IOC.Register<IFoo>(() => { return new FooImpl(); });
            var theFoo = IOC.Resolve<IFoo>();
            Assert.IsTrue(theFoo is FooImpl);
        }

        [TestMethod]
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

        [TestMethod]
        public void ImportAttribute_ResolvedAutomatically()
        {
            var imp = new Importeur();
            IOC.ResolveImports(imp);
            Assert.IsNotNull(imp.importedFnord);
            Assert.IsTrue(imp.importedFnord is FnordImpl);
            Assert.IsNull(imp.notImportedFnord);
        }

        [TestMethod]
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

        [TestMethod]
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
