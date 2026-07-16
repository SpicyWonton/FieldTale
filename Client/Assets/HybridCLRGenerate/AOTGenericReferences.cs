using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Fantasy.Unity.dll",
		"GameFramework.dll",
		"System.Core.dll",
		"System.dll",
		"UnityEngine.CoreModule.dll",
		"UnityGameFramework.Runtime.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Fantasy.Async.AsyncFTaskMethodBuilder<object>
	// Fantasy.Async.FTask<object>
	// Fantasy.Network.Interface.Message<object>
	// Fantasy.Pool.MessageObjectPool<object>
	// GameFramework.GameFrameworkLinkedList.Enumerator<object>
	// GameFramework.GameFrameworkLinkedList<object>
	// LightProto.IProtoParser<object>
	// LightProto.IProtoReader<object>
	// LightProto.IProtoWriter<object>
	// LightProto.Parser.ICollectionReader<object>
	// LightProto.Parser.MessageWrapper.ProtoReader<object>
	// LightProto.Parser.MessageWrapper.ProtoWriter<object>
	// System.Action<FieldTale.HotUpdate.Player.InputCommand>
	// System.Action<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Action<byte>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment<byte>
	// System.ByReference<byte>
	// System.Collections.Generic.ArraySortHelper<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.ArraySortHelper<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.Comparer<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<long,int>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,int>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,int>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.ICollection<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.IComparer<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.IEnumerable<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.IEnumerator<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.IList<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,int>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.List.Enumerator<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.List<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.Generic.ObjectComparer<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<FieldTale.HotUpdate.Player.InputCommand>
	// System.Collections.ObjectModel.ReadOnlyCollection<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<FieldTale.HotUpdate.Player.InputCommand>
	// System.Comparison<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Comparison<object>
	// System.EventHandler<object>
	// System.Func<object,object>
	// System.Func<object,uint,object,object>
	// System.Func<object>
	// System.Nullable<uint>
	// System.Predicate<FieldTale.HotUpdate.Player.InputCommand>
	// System.Predicate<FieldTale.HotUpdate.Player.NetworkSnapshot>
	// System.Predicate<object>
	// System.ReadOnlySpan<byte>
	// System.Span<byte>
	// }}

	public void RefMethods()
	{
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.AwaitUnsafeOnCompleted<Fantasy.Async.FTaskCompleted,FieldTale.HotUpdate.M2C_PlayerCreateHandler.<Run>d__0>(Fantasy.Async.FTaskCompleted&,FieldTale.HotUpdate.M2C_PlayerCreateHandler.<Run>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.AwaitUnsafeOnCompleted<Fantasy.Async.FTaskCompleted,FieldTale.HotUpdate.M2C_PlayerMoveHandler.<Run>d__0>(Fantasy.Async.FTaskCompleted&,FieldTale.HotUpdate.M2C_PlayerMoveHandler.<Run>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.AwaitUnsafeOnCompleted<object,LoginForm.<OnBtnLoginClicked>d__9>(object&,LoginForm.<OnBtnLoginClicked>d__9&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__0>(object&,Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__1>(object&,Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__1&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.Start<FieldTale.HotUpdate.M2C_PlayerCreateHandler.<Run>d__0>(FieldTale.HotUpdate.M2C_PlayerCreateHandler.<Run>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.Start<FieldTale.HotUpdate.M2C_PlayerMoveHandler.<Run>d__0>(FieldTale.HotUpdate.M2C_PlayerMoveHandler.<Run>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder.Start<LoginForm.<OnBtnLoginClicked>d__9>(LoginForm.<OnBtnLoginClicked>d__9&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder<object>.Start<Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__0>(Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__0&)
		// System.Void Fantasy.Async.AsyncFTaskMethodBuilder<object>.Start<Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__1>(Fantasy.NetworkProtocolHelper.<C2G_LoginGameRequest>d__1&)
		// Fantasy.Async.FTask<Fantasy.Network.Interface.IResponse> Fantasy.Network.Session.Call<object>(object,long)
		// System.Void Fantasy.Network.Session.Send<object>(object,uint,long)
		// System.Void GameFramework.Fsm.Fsm<object>.ChangeState<object>()
		// System.Void GameFramework.Fsm.FsmState<object>.ChangeState<object>(GameFramework.Fsm.IFsm<object>)
		// object GameFramework.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void GameFramework.Fsm.IFsm<object>.SetData<object>(string,object)
		// System.Void GameFramework.GameFrameworkLog.Error<object,byte,object>(string,object,byte,object)
		// System.Void GameFramework.GameFrameworkLog.Error<object,object>(string,object,object)
		// System.Void GameFramework.GameFrameworkLog.Error<object>(string,object)
		// System.Void GameFramework.GameFrameworkLog.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void GameFramework.GameFrameworkLog.Info<object,object>(string,object,object)
		// System.Void GameFramework.GameFrameworkLog.Info<object>(string,object)
		// System.Void GameFramework.Procedure.IProcedureManager.StartProcedure<object>()
		// string GameFramework.Utility.Text.Format<int>(string,int)
		// string GameFramework.Utility.Text.Format<object,byte,object>(string,object,byte,object)
		// string GameFramework.Utility.Text.Format<object,object,object,object>(string,object,object,object,object)
		// string GameFramework.Utility.Text.Format<object,object>(string,object,object)
		// string GameFramework.Utility.Text.Format<object>(string,object)
		// string GameFramework.Utility.Text.Format<uint>(string,uint)
		// string GameFramework.Utility.Text.ITextHelper.Format<int>(string,int)
		// string GameFramework.Utility.Text.ITextHelper.Format<object,byte,object>(string,object,byte,object)
		// string GameFramework.Utility.Text.ITextHelper.Format<object,object,object,object>(string,object,object,object,object)
		// string GameFramework.Utility.Text.ITextHelper.Format<object,object>(string,object,object)
		// string GameFramework.Utility.Text.ITextHelper.Format<object>(string,object)
		// string GameFramework.Utility.Text.ITextHelper.Format<uint>(string,uint)
		// int LightProto.Serializer.CalculateMessageSize<object>(LightProto.IProtoWriter<object>,object)
		// object LightProto.Serializer.Deserialize<object>(System.IO.Stream,LightProto.IProtoReader<object>)
		// object LightProto.Serializer.ParseMessageFrom<object>(LightProto.IProtoReader<object>,LightProto.ReaderContext&)
		// System.Void LightProto.Serializer.Serialize<object>(System.Buffers.IBufferWriter<byte>,object,LightProto.IProtoWriter<object>)
		// System.Void LightProto.Serializer.WriteMessageTo<object>(LightProto.IProtoWriter<object>,LightProto.WriterContext&,object)
		// System.RuntimeTypeHandle[] System.Array.Empty<System.RuntimeTypeHandle>()
		// object[] System.Array.Empty<object>()
		// uint[] System.Array.Empty<uint>()
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityGameFramework.Runtime.Log.Error<object,byte,object>(string,object,byte,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object>(string,object)
	}
}