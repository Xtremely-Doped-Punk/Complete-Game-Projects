using Unity.Netcode.Components;

public class OwnerNetworkAnimator : NetworkAnimator
{
    /*
     * Usually, your project's design (or personal preference) might require that owners are immediately updated to any Animator state changes. 
     * The most typical reason would be to give the local player with instantaneous visual (animation) feedback. 
     * To create an owner authoritative NetworkAnimator you need to create a new class that's derived from NetworkAnimator, 
     * override the NetworkAnimator.OnIsServerAuthoritative method, and within the overridden OnIsServerAuthoritative method you should return false
     */
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
    /*
     * This is similar case as Network-Transform vs Client-Network-Transform
     * source: https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator
     */
}