using MonoGame.Extended.Entities;

namespace MonoGame.Utils.Extensions
{
    public static class ComponentMapperExtensions
    {

        public static bool TryGet<T>(this ComponentMapper<T> componentMapper, int entityId, out T component) where T : class
        {
            if (componentMapper.Has(entityId))
            {
                component = componentMapper.Get(entityId);
                return true;
            }

            component = null;
            return false;
        }

    }
}
