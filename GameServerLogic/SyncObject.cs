using System;
using System.Numerics;

namespace WebSocketServer.GameServerLogic
{
    public class SyncObject
    {
        public int ID;
        public string name;

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 localScale;

        public SyncObject(int _id, string _name, Vector3 _position, Vector3 _localScale)
        {
            ID = _id;
            name = _name;
            position = _position;
            velocity = Vector3.Zero;
            localScale = _localScale;
        }

        public void Sync(Vector3 _pos, Vector3 _vel, Vector3 _size)
        {
            position = _pos;
            velocity = _vel;
            localScale = _size;
        }
    }
}
