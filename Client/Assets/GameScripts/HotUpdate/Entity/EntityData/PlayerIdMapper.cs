using System.Collections.Generic;

namespace FieldTale.HotUpdate
{
    public static class PlayerIdMapper
    {
        private static readonly Dictionary<long, int> s_PlayerEntityIds = new Dictionary<long, int>();
        private static int s_NextEntityId = 1;

        public static int GetOrCreate(long playerId)
        {
            if (s_PlayerEntityIds.TryGetValue(playerId, out int entityId))
            {
                return entityId;
            }

            entityId = s_NextEntityId++;
            s_PlayerEntityIds.Add(playerId, entityId);
            return entityId;
        }

        public static bool Remove(long playerId)
        {
            return s_PlayerEntityIds.Remove(playerId);
        }
    }
}