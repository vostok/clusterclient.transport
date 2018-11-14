using System;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Clusterclient.Transport.Tests
{
    internal class Translator_Tests
    {
        [Test]
        public void Should_copy_public_properties_with_same_names()
        {
            var a = new A
            {
                N = 4,
                S = "abacaba",
                T = TimeSpan.FromHours(5)
            };

            var expected = new B
            {
                N = 4,
                S = "abacaba",
                T = TimeSpan.FromHours(5)
            };

            var actual = Translator.Translate<B>(a);

            actual.Should().BeEquivalentTo(expected);
        }

        private class A
        {
            public int N { get; set; }
            public string S { get; set; }
            public TimeSpan? T { get; set; }
        }

        private class B
        {
            public int N { get; set; }
            public string S { get; set; }
            public TimeSpan? T { get; set; }
        }
    }
}