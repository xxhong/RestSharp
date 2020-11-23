//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RestSharp.Serializers.Json.SimpleJson {
    [GeneratedCode("reflection-utils", "1.0.0")]
    class ReflectionUtils {
        public delegate object GetDelegate(object source);

        public delegate TValue ThreadSafeDictionaryValueFactory<in TKey, out TValue>(TKey key);

        static Type GetTypeInfo(Type type) => type;

        static bool IsValueType(Type type) => GetTypeInfo(type).IsValueType;

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
            => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<FieldInfo> GetFields(Type type)
            => type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo) => propertyInfo.GetGetMethod(true);

        public static GetDelegate GetGetMethod(PropertyInfo propertyInfo) => GetGetMethodByExpression(propertyInfo);

        public static GetDelegate GetGetMethod(FieldInfo fieldInfo) => GetGetMethodByExpression(fieldInfo);

        static GetDelegate GetGetMethodByExpression(PropertyInfo propertyInfo) {
            var getMethodInfo = GetGetterMethodInfo(propertyInfo);
            var instance      = Expression.Parameter(typeof(object), "instance");

            var instanceCast = (!IsValueType(propertyInfo.DeclaringType!))
                ? Expression.TypeAs(instance, propertyInfo.DeclaringType)
                : Expression.Convert(instance, propertyInfo.DeclaringType);

            var compiled = Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(Expression.Call(instanceCast, getMethodInfo), typeof(object)),
                    instance
                )
                .Compile();
            return source => compiled(source);
        }

        static GetDelegate GetGetMethodByExpression(FieldInfo fieldInfo) {
            var instance = Expression.Parameter(typeof(object), "instance");
            var member   = Expression.Field(Expression.Convert(instance, fieldInfo.DeclaringType!), fieldInfo);
            var compiled = Expression.Lambda<GetDelegate>(Expression.Convert(member, typeof(object)), instance).Compile();
            return source => compiled(source);
        }

        public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
            readonly object _lock = new();

            readonly ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;

            Dictionary<TKey, TValue>? _dictionary;

            public ThreadSafeDictionary(ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory) => _valueFactory = valueFactory;

            TValue Get(TKey key)
                => _dictionary == null ? AddValue(key) : !_dictionary.TryGetValue(key, out var value) ? AddValue(key) : value;

            TValue AddValue(TKey key) {
                var value = _valueFactory(key);

                lock (_lock) {
                    if (_dictionary == null) {
                        _dictionary = new Dictionary<TKey, TValue> {[key] = value};
                    }
                    else {
                        if (_dictionary.TryGetValue(key, out var val)) return val;

                        var dict = new Dictionary<TKey, TValue>(_dictionary) {[key] = value};
                        _dictionary = dict;
                    }
                }

                return value;
            }

            public void Add(TKey key, TValue value) => throw new NotImplementedException();

            public bool ContainsKey(TKey key) => _dictionary != null && _dictionary.ContainsKey(key);

            public ICollection<TKey>? Keys => _dictionary?.Keys;

            public bool Remove(TKey key) => throw new NotImplementedException();

            public bool TryGetValue(TKey key, out TValue value) {
                value = this[key];
                return true;
            }

            public ICollection<TValue>? Values => _dictionary?.Values;

            public TValue this[TKey key] {
                get => Get(key);
                set => throw new NotImplementedException();
            }

            public void Add(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

            public void Clear() => throw new NotImplementedException();

            public bool Contains(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

            public int Count => _dictionary?.Count ?? 0;

            public bool IsReadOnly => throw new NotImplementedException();

            public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

            public IEnumerator<KeyValuePair<TKey, TValue>>? GetEnumerator() => _dictionary?.GetEnumerator();

            IEnumerator? IEnumerable.GetEnumerator() => _dictionary?.GetEnumerator();
        }
    }
}
