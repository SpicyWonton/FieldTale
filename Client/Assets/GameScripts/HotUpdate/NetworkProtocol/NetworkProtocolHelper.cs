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

   }
}