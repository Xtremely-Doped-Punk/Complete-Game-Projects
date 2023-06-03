using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority
{
    /// <summary>
    /// Used for syncing a transform with client side changes. This includes host. Pure server as owner isn't supported by this. Please use NetworkTransform
    /// for transforms that'll always be owned by the server.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        /// <summary>
        /// Used to determine who can write to this transform. Owner client only.
        /// This imposes state to the server. This is putting trust on your clients. Make sure no security-sensitive features use this transform.
        /// </summary>

        /*
         * Owner authority of a NetworkTransform is dictated by the NetworkTransform.OnIsServerAuthoritative method 
         * when a NetworkTransform component is first initialized. If it returns true (the default) then 
         * it initializes as a server authoritative NetworkTransform. If it returns false then 
         * it initializes as an owner authoritative NetworkTransform (a.k.a. ClientNetworkTransform). 
         * This can be achieved by deriving from NetworkTransform, overriding the OnIsServerAuthoritative virtual method, and returning false
         */
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
        /*
         * By default, NetworkTransform operates in server authoritative mode. 
         * This means that changes to transform axis (marked to be synchronized) 
         * are detected on the server-side and pushed to connected clients.
         * source: https://docs-multiplayer.unity3d.com/netcode/current/components/networktransform
         */
    }
}