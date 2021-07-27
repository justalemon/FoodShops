using GTA;
using GTA.Native;

namespace FoodShops.Locations
{
    /// <summary>
    /// Just some tools to keep things DRY.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Plays a specific animation and waits until this animation is over.
        /// </summary>
        /// <param name="animDict">The animation dictionary to use.</param>
        /// <param name="animName">The name of the animation to use.</param>
        /// <param name="animFlags">The flags to apply to the animation.</param>
        public static void PlayAnimationAndWait(string animDict, string animName, AnimationFlags animFlags)
        {
            Ped ped = Game.Player.Character;

            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Script.Yield();
            }

            Game.Player.Character.Task.PlayAnimation(animDict, animName, 8f, -8f, -1, animFlags, 0f);

            while (Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, ped, animDict, animName) < 0.9f)
            {
                Script.Yield();
            }
        }
    }
}
