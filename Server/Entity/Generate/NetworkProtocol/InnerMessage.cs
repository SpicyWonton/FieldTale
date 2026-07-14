using LightProto;
using MemoryPack;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Fantasy;
using Fantasy.Pool;
using Fantasy.Network.Interface;
using Fantasy.Serialize;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable RedundantNameQualifier
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable RedundantUsingDirective
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618
namespace Fantasy
{
    /// <summary>
    /// 通知Map进行下线操作
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class G2M_OfflineRequest : AMessage, IRoamingRequest
    {
        public static G2M_OfflineRequest Create(bool autoReturn = true)
        {
            var g2M_OfflineRequest = MessageObjectPool<G2M_OfflineRequest>.Rent();
            g2M_OfflineRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2M_OfflineRequest.SetIsPool(false);
            }
            
            return g2M_OfflineRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            OfflineTime = default;
            MessageObjectPool<G2M_OfflineRequest>.Return(this);
        }
        public uint OpCode() { return InnerOpcode.G2M_OfflineRequest; } 
        [ProtoIgnore]
        public M2G_OfflineResponse ResponseType { get; set; }
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
        [ProtoMember(1)]
        public int OfflineTime { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class M2G_OfflineResponse : AMessage, IRoamingResponse
    {
        public static M2G_OfflineResponse Create(bool autoReturn = true)
        {
            var m2G_OfflineResponse = MessageObjectPool<M2G_OfflineResponse>.Rent();
            m2G_OfflineResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                m2G_OfflineResponse.SetIsPool(false);
            }
            
            return m2G_OfflineResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            MessageObjectPool<M2G_OfflineResponse>.Return(this);
        }
        public uint OpCode() { return InnerOpcode.M2G_OfflineResponse; } 
        [ProtoMember(1)]
        public uint ErrorCode { get; set; }
    }
}