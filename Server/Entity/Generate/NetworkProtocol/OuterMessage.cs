using LightProto;
using System;
using MemoryPack;
using System.Collections.Generic;
using Fantasy;
using Fantasy.Pool;
using Fantasy.Network.Interface;
using Fantasy.Serialize;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618
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
namespace Fantasy
{
    /// <summary>
    /// 客户端登陆到Gate服务器
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class C2G_LoginGameRequest : AMessage, IRequest
    {
        public static C2G_LoginGameRequest Create(bool autoReturn = true)
        {
            var c2G_LoginGameRequest = MessageObjectPool<C2G_LoginGameRequest>.Rent();
            c2G_LoginGameRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_LoginGameRequest.SetIsPool(false);
            }
            
            return c2G_LoginGameRequest;
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
            Account = default;
            Password = default;
            MessageObjectPool<C2G_LoginGameRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_LoginGameRequest; } 
        [ProtoIgnore]
        public G2C_LoginGameResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public string Account { get; set; }
        [ProtoMember(2)]
        public string Password { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_LoginGameResponse : AMessage, IResponse
    {
        public static G2C_LoginGameResponse Create(bool autoReturn = true)
        {
            var g2C_LoginGameResponse = MessageObjectPool<G2C_LoginGameResponse>.Rent();
            g2C_LoginGameResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_LoginGameResponse.SetIsPool(false);
            }
            
            return g2C_LoginGameResponse;
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
            MessageObjectPool<G2C_LoginGameResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_LoginGameResponse; } 
        [ProtoMember(1)]
        public uint ErrorCode { get; set; }
    }
    /// <summary>
    /// 客户端通知服务器可以接收服务器推送的消息
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class C2M_InitComplete : AMessage, IRoamingMessage
    {
        public static C2M_InitComplete Create(bool autoReturn = true)
        {
            var c2M_InitComplete = MessageObjectPool<C2M_InitComplete>.Rent();
            c2M_InitComplete.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2M_InitComplete.SetIsPool(false);
            }
            
            return c2M_InitComplete;
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
            MessageObjectPool<C2M_InitComplete>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2M_InitComplete; } 
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
    }
    /// <summary>
    /// Map服务器通知客户端创建新的Player
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class M2C_PlayerCreate : AMessage, IRoamingMessage
    {
        public static M2C_PlayerCreate Create(bool autoReturn = true)
        {
            var m2C_PlayerCreate = MessageObjectPool<M2C_PlayerCreate>.Rent();
            m2C_PlayerCreate.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                m2C_PlayerCreate.SetIsPool(false);
            }
            
            return m2C_PlayerCreate;
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
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }
            IsSelf = default;
            MessageObjectPool<M2C_PlayerCreate>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.M2C_PlayerCreate; } 
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
        [ProtoMember(1)]
        public PlayerInfo Player { get; set; }
        [ProtoMember(2)]
        public bool IsSelf { get; set; }
    }
    /// <summary>
    /// 客户端上报移动输入，玩家身份由服务器当前连接确定
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class C2M_PlayerMove : AMessage, IRoamingMessage
    {
        public static C2M_PlayerMove Create(bool autoReturn = true)
        {
            var c2M_PlayerMove = MessageObjectPool<C2M_PlayerMove>.Rent();
            c2M_PlayerMove.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2M_PlayerMove.SetIsPool(false);
            }
            
            return c2M_PlayerMove;
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
            ClientTick = default;
            MoveX = default;
            MoveY = default;
            MessageObjectPool<C2M_PlayerMove>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2M_PlayerMove; } 
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
        [ProtoMember(1)]
        public uint ClientTick { get; set; }
        [ProtoMember(2)]
        public int MoveX { get; set; }
        [ProtoMember(3)]
        public int MoveY { get; set; }
    }
    /// <summary>
    /// Map服务器广播玩家权威移动状态
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class M2C_PlayerMove : AMessage, IRoamingMessage
    {
        public static M2C_PlayerMove Create(bool autoReturn = true)
        {
            var m2C_PlayerMove = MessageObjectPool<M2C_PlayerMove>.Rent();
            m2C_PlayerMove.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                m2C_PlayerMove.SetIsPool(false);
            }
            
            return m2C_PlayerMove;
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
            PlayerId = default;
            if (Pos != null)
            {
                Pos.Dispose();
                Pos = null;
            }
            ServerTick = default;
            LastProcessedClientTick = default;
            MessageObjectPool<M2C_PlayerMove>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.M2C_PlayerMove; } 
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public Position Pos { get; set; }
        [ProtoMember(3)]
        public uint ServerTick { get; set; }
        [ProtoMember(4)]
        public uint LastProcessedClientTick { get; set; }
    }
    /// <summary>
    /// Map通知客户端有Player离开
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class M2C_PlayerLeave : AMessage, IRoamingMessage
    {
        public static M2C_PlayerLeave Create(bool autoReturn = true)
        {
            var m2C_PlayerLeave = MessageObjectPool<M2C_PlayerLeave>.Rent();
            m2C_PlayerLeave.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                m2C_PlayerLeave.SetIsPool(false);
            }
            
            return m2C_PlayerLeave;
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
            PlayerId = default;
            MessageObjectPool<M2C_PlayerLeave>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.M2C_PlayerLeave; } 
        [ProtoIgnore]
        public int RouteType => Fantasy.RoamingType.MapRoamingType;
        [ProtoMember(1)]
        public long PlayerId { get; set; }
    }
    /// <summary>
    /// Player信息
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class PlayerInfo : AMessage, IDisposable
    {
        public static PlayerInfo Create(bool autoReturn = true)
        {
            var playerInfo = MessageObjectPool<PlayerInfo>.Rent();
            playerInfo.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                playerInfo.SetIsPool(false);
            }
            
            return playerInfo;
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
            PlayerId = default;
            Name = default;
            if (Pos != null)
            {
                Pos.Dispose();
                Pos = null;
            }
            MessageObjectPool<PlayerInfo>.Return(this);
        }
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public Position Pos { get; set; }
    }
    /// <summary>
    /// 坐标信息
    /// </summary>
    [Serializable]
    [ProtoContract]
    public partial class Position : AMessage, IDisposable
    {
        public static Position Create(bool autoReturn = true)
        {
            var position = MessageObjectPool<Position>.Rent();
            position.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                position.SetIsPool(false);
            }
            
            return position;
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
            X = default;
            Y = default;
            Z = default;
            MessageObjectPool<Position>.Return(this);
        }
        [ProtoMember(1)]
        public float X { get; set; }
        [ProtoMember(2)]
        public float Y { get; set; }
        [ProtoMember(3)]
        public float Z { get; set; }
    }
}