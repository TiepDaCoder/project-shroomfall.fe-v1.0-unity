using Contract.DTO.Abstraction;
using Contract.DTO.Runtime.EntityDomain.Component;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Assets.Source.Utility
{
    public class ComponentInstanceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComponentInstanceDTO);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var type = obj
                .Properties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.Name,
                        "ComponentType",
                        StringComparison.OrdinalIgnoreCase))
                ?.Value?
                .Value<string>();

            ComponentInstanceDTO result = type switch
            {
                "TransformInstanceDTO" => new TransformInstanceDTO(),
                "AppearanceInstanceDTO" => new AppearanceInstanceDTO(),
                "CollisionInstanceDTO" => new CollisionInstanceDTO(),
                "CharacteristicInstanceDTO" => new CharacteristicInstanceDTO(),
                "OwnershipInstanceDTO" => new OwnershipInstanceDTO(),
                "InventoryInstanceDTO" => new InventoryInstanceDTO(),
                "ActionInstanceDTO" => new ActionInstanceDTO(),
                "EffectContainerInstanceDTO" => new EffectContainerInstanceDTO(),
                _ => new ComponentInstanceDTO()
            };

            serializer.Populate(obj.CreateReader(), result);

            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
