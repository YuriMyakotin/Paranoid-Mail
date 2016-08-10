using System;
using static Paranoid.Utils;
using Chaos.NaCl;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
	public partial class NetSessionServer : NetSessionExtended
	{
		private void SignRoutedMessage(Message RoutedMsg,long UserID)
		{
			Blake256 Bl=new Blake256();
			Bl.TransformLong(UserID);
			Bl.TransformBytes(RoutedMsg.MessageBody, 200, 56);
			Bl.TransformLong(UserID);
			byte[] Hash = (Bl.TransformFinal()).GetBytes();
			Bl.Initialize();

			Buffer.BlockCopy(Hash,0,RoutedMsg.MessageBody,64,32);

			ArraySegment<byte> Sig = new ArraySegment<byte>(RoutedMsg.MessageBody, 0, 64);
			ArraySegment<byte> Body = new ArraySegment<byte>(RoutedMsg.MessageBody, 64, RoutedMsg.MessageBody.Length - 64);
			Ed25519.Sign(Sig,Body,new ArraySegment<byte>(Ed25519.ExpandedPrivateKeyFromSeed(MySecretKey)));
			RoutedMsg.ToUser = 0;
			RoutedMsg.FromUser = 0;
			RoutedMsg.MessageID = MakeMsgID();


		}

		private long RoutedMessageGetToUser(Message RoutedMsg)
		{

			byte[] RemotePubKey = new byte[32];
			Buffer.BlockCopy(RoutedMsg.MessageBody, 96, RemotePubKey, 0, 32);
			byte[] SharedKey = Ed25519.KeyExchange(RemotePubKey, Ed25519.ExpandedPrivateKeyFromSeed(MySecretKey));

			Blake256 Bl = new Blake256();
			Bl.TransformBytes(SharedKey);
			Bl.TransformBytes(RoutedMsg.MessageBody, 200, 56);
			Bl.TransformBytes(SharedKey);
			byte[] Hash = (Bl.TransformFinal()).GetBytes();
			Bl.Initialize();

			return BitConverter.ToInt64(RoutedMsg.MessageBody, 128) ^ BitConverter.ToInt64(Hash, 0) ^
			              BitConverter.ToInt64(Hash, 8) ^
			              BitConverter.ToInt64(Hash, 16) ^
			              BitConverter.ToInt64(Hash, 24);

		}


	}
}