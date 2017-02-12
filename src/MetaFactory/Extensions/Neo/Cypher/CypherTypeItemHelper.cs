﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
    public class CypherTypeItemHelper
    {
        private readonly ConcurrentDictionary<CypherTypeItem, List<CypherProperty>> _typeProperties = new ConcurrentDictionary<CypherTypeItem, List<CypherProperty>>();

        public CypherTypeItem AddKeyAttribute<TEntity, TAttr>(ICypherExtensionContext context, TEntity entity)
            where TAttr : CypherExtensionAttribute
            where TEntity : class
        {
            var type = entity.GetType();
            var key = new CypherTypeItem { Type = type, AttributeType = typeof(TAttr) };
            //check cache
            if (!_typeProperties.ContainsKey(key))
            {
                var properties = type.GetTypeInfo().GetProperties();
                //strip off properties create map for usage
                _typeProperties.AddOrUpdate(key, properties.Where(x => x.GetCustomAttributes(typeof(TAttr),true).Any())
                    .Select(x => new CypherProperty {TypeName = x.Name, JsonName = x.Name.ApplyCasing(context)})
                    .ToList(), (k, e) => e);
            }
            return key;
        }

        public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>(ICypherExtensionContext context, TEntity entity)
            where TEntity : class
            where TAttr : CypherExtensionAttribute
        {
            var key = AddKeyAttribute<TEntity, TAttr>(context, entity);
            return _typeProperties[key];
        }

        public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>(TEntity entity)
            where TEntity : class
            where TAttr : CypherExtensionAttribute
        {
            return PropertiesForPurpose<TEntity, TAttr>(CypherExtension.DefaultExtensionContext,entity);
        }

        public void AddPropertyUsage(CypherTypeItem type, List<CypherProperty> properties)
        {
            _typeProperties.AddOrUpdate(type, properties, (item, list) => properties);
        }
    }
}
