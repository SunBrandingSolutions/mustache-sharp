using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Mustache.Test
{
    public class UpcastDictionaryTester
    {
        [Fact]
        public void ShouldReturnNullForNull()
        {
            IDictionary<string, object> result = UpcastDictionary.Create(null);
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnArgumentIfIDictionary_string_object()
        {
            object source = new Dictionary<string, object>();
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Same(source, result);
        }

        [Fact]
        public void ShouldReturnNullIfNotGenericType()
        {
            object source = String.Empty;
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnNullIfWrongNumberOfGenericArguments()
        {
            object source = new List<string>();
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnNullIfFirstGenericTypeArgumentIsNotAString()
        {
            object source = new Dictionary<object, object>();
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnNullIfNotDictionaryType()
        {
            object source = (Func<string, object>)(s => (object)s);
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnUpcastWrapperForDictionary_string_TValue()
        {
            object source = new Dictionary<string, string>();
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.IsType<UpcastDictionary<string>>(result);
        }

        [Fact]
        public void ShouldFindKeyIfInWrappedDictionary()
        {
            object source = new Dictionary<string, string>() { { "Name", "Bob" } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            bool containsKey = result.ContainsKey("Name");
            Assert.True(containsKey);
        }

        [Fact]
        public void ShouldNotFindKeyIfNotInWrappedDictionary()
        {
            object source = new Dictionary<string, string>() { { "Name", "Bob" } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            bool containsKey = result.ContainsKey("Age");
            Assert.False(containsKey);
        }

        [Fact]
        public void ShouldFindKeysInWrappedDictionary()
        {
            var source = new Dictionary<string, string>() { { "Name", "Bob" }, { "Age", "100" } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            ICollection sourceKeys = source.Keys;
            ICollection wrappedKeys = result.Keys.ToArray();
            Assert.Equal(sourceKeys, wrappedKeys);
        }

        [Fact]
        public void ShouldFindKeyIfInWrappedDictionary_TryGetValue()
        {
            var source = new Dictionary<string, string>() { { "Name", "Bob" } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            object value;
            bool found = result.TryGetValue("Name", out value);
            Assert.True(found);
            Assert.Same(source["Name"], value);
        }

        [Fact]
        public void ShouldNotFindKeyIfNotInWrappedDictionary_TryGetValue()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            object value;
            bool found = result.TryGetValue("Name", out value);
            Assert.False(found, "The key should not have been found.");
            Assert.Null(value);
        
        }

        [Fact]
        public void ShouldReturnValuesAsObjects()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 }, { "Weight", 500 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            ICollection sourceValues = source.Values;
            ICollection wrappedValues = result.Values.ToArray();
            Assert.Equal(sourceValues, wrappedValues);
        }

        [Fact]
        public void ShouldFindKeyIfInWrappedDictionary_Indexer()
        {
            var source = new Dictionary<string, string>() { { "Name", "Bob" } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            object value = result["Name"];
            Assert.Same(source["Name"], value);
        }

        [Fact]
        public void ShouldNotFindKeyIfNotInWrappedDictionary_Indexer()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var source = new Dictionary<string, int>() { { "Age", 100 } };
                IDictionary<string, object> result = UpcastDictionary.Create(source);
                object value = result["Name"];
            });
        }

        [Fact]
        public void ShouldNotFindPairIfValueWrongType()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            bool contains = result.Contains(new KeyValuePair<string, object>("Age", "Blah"));
            Assert.False(contains, "The pair should not have been found.");
        }

        [Fact]
        public void ShouldFindPairInWrappedDictionary()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            bool contains = result.Contains(new KeyValuePair<string, object>("Age", 100));
            Assert.True(contains, "The pair should have been found.");
        }

        [Fact]
        public void ShouldCopyPairsToArray()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 }, { "Weight", 45 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            var array = new KeyValuePair<string, object>[2];
            result.CopyTo(array, 0);
            var expected = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("Age", 100),
                new KeyValuePair<string, object>("Weight", 45)
            };
            Assert.Equal(expected, array);
        }

        [Fact]
        public void ShouldGetCount()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 }, { "Weight", 45 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            Assert.Equal(source.Count, result.Count);
        }

        [Fact]
        public void ShouldGetEnumerator()
        {
            var source = new Dictionary<string, int>() { { "Age", 100 }, { "Weight", 45 } };
            IDictionary<string, object> result = UpcastDictionary.Create(source);
            IEnumerator<KeyValuePair<string, object>> enumerator = result.GetEnumerator();
            var values = new List<KeyValuePair<string, object>>();
            while (enumerator.MoveNext())
            {
                values.Add(enumerator.Current);
            }
            var expected = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("Age", 100),
                new KeyValuePair<string, object>("Weight", 45)
            };
            Assert.Equal(expected, values);
        }

        /// <summary>
        /// Newtonsoft's JSON.NET has an object called JObject. This is a concrete class
        /// that inherits from IDictionary&lt;string, JToken&gt;. The UpcastDictionary
        /// should be able to handle this type.
        /// </summary>
        [Fact]
        public void ShouldHandleConcreteClassInheritingFromDictionary()
        {
            var dictionary = new ConcreteDictionary() { { "Name", "Bob" } };
            var result = UpcastDictionary.Create(dictionary);
            Assert.Equal(dictionary["Name"], result["Name"]);
        }

        public class ConcreteDictionary : Dictionary<string, string>
        {
        }
    }
}
