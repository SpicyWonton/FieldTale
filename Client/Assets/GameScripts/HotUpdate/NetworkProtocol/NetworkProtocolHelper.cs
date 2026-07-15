using System.Runtime.CompilerServices;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using System.Collections.Generic;
#pragma warning disable CS8618
namespace Fantasy
{
   public static class NetworkProtocolHelper
   {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_LoginGameResponse> C2G_LoginGameRequest(this Session session, C2G_LoginGameRequest C2G_LoginGameRequest_request)
		{
			return (G2C_LoginGameResponse)await session.Call(C2G_LoginGameRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_LoginGameResponse> C2G_LoginGameRequest(this Session session, string account, string password)
		{
			using var C2G_LoginGameRequest_request = Fantasy.C2G_LoginGameRequest.Create();
			C2G_LoginGameRequest_request.Account = account;
			C2G_LoginGameRequest_request.Password = password;
			return (G2C_LoginGameResponse)await session.Call(C2G_LoginGameRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C2M_InitComplete(this Session session, C2M_InitComplete C2M_InitComplete_message)
		{
			session.Send(C2M_InitComplete_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C2M_InitComplete(this Session session)
		{
			using var message = Fantasy.C2M_InitComplete.Create();
			session.Send(message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void M2C_PlayerCreate(this Session session, M2C_PlayerCreate M2C_PlayerCreate_message)
		{
			session.Send(M2C_PlayerCreate_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void M2C_PlayerCreate(this Session session, PlayerInfo player, bool isSelf)
		{
			using var M2C_PlayerCreate_message = Fantasy.M2C_PlayerCreate.Create();
			M2C_PlayerCreate_message.Player = player;
			M2C_PlayerCreate_message.IsSelf = isSelf;
			session.Send(M2C_PlayerCreate_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void M2C_PlayerLeave(this Session session, M2C_PlayerLeave M2C_PlayerLeave_message)
		{
			session.Send(M2C_PlayerLeave_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void M2C_PlayerLeave(this Session session, long playerId)
		{
			using var M2C_PlayerLeave_message = Fantasy.M2C_PlayerLeave.Create();
			M2C_PlayerLeave_message.PlayerId = playerId;
			session.Send(M2C_PlayerLeave_message);
		}

   }
}