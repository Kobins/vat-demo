using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class VATStateMachineBehaviour : StateMachineBehaviour
    {
        private static readonly Dictionary<int, string> StateNameByHash =
            new List<string>()
            {
                "backwards-run", "backwards-walk", "idle", "jump-down",
                "jump-float", "jump-up", "pickup", "run", "victory", "walk", "wave"
            }.ToDictionary(Animator.StringToHash, it => it);

        private static string GetStateNameByHash(int hash) => StateNameByHash.TryGetValue(hash, out var result) ? result : $"unknown({hash})";


        private static string ToString(AnimatorClipInfo[] clipInfo) =>
            string.Join(", ", clipInfo.Select(it => $"{it.clip.name}={(int)(it.weight*100)}%"));
        /*
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var currentClipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
            var nextClipInfo = animator.GetNextAnimatorClipInfo(layerIndex);

            Debug.Log($"[{layerIndex}] ENTER - normalized: {stateInfo.normalizedTime}, current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");   
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var currentClipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
            var nextClipInfo = animator.GetNextAnimatorClipInfo(layerIndex);

            Debug.Log($"[{layerIndex}] EXIT - normalized: {stateInfo.normalizedTime}, current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");   
        }
        */

        private VATController vat;

        /*
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var currentClipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
            var nextClipInfo = animator.GetNextAnimatorClipInfo(layerIndex);
            Debug.Log($"[{layerIndex}] ENTER - normalized: {stateInfo.normalizedTime}, current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");
        }
        */

        private static AnimatorClipInfo MaxWeightClip(AnimatorClipInfo[] clipInfos, out float weightSum)
        {
            ref AnimatorClipInfo maxWeight = ref clipInfos[0];
            weightSum = maxWeight.weight;
            for (var index = 1; index < clipInfos.Length; index++)
            {
                var clipInfo = clipInfos[index];
                weightSum += clipInfo.weight;
                if (clipInfo.weight > maxWeight.weight)
                {
                    maxWeight = clipInfo;
                }
            }

            return maxWeight;
        }

        private int GetAnimationIndexByClip(AnimationClip clip)
        {
            if (!vat) return -1;
            return vat.data.ClipByName.TryGetValue(clip.name, out var clipData) ? clipData.index : -1;
        }
        
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(true)
                return;
            var currentClipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
            var nextClipInfo = animator.GetNextAnimatorClipInfo(layerIndex);
            // Debug.Log($"[{layerIndex}] UPDATE - normalized: {stateInfo.normalizedTime}, current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");

            if (!vat)
            {
                vat = animator.GetComponentInChildren<VATController>();
                if(!vat) return;
            }   
            
            // 현재 실행중인 애니메이션 클립이 없음: 아무것도 안 함
            if (currentClipInfo.Length <= 0)
            {
                return;
            }

            float blendFactor;
            // 다음 애니메이션이 있음: 애니메이션 블렌딩 필요
            if (nextClipInfo.Length >= 1)
            {
                // 최대 weight 애니메이션만 사용
                var prev = MaxWeightClip(currentClipInfo, out _);
                var current = MaxWeightClip(nextClipInfo, out blendFactor);
                
                vat.PrevAnimationIndex = GetAnimationIndexByClip(prev.clip);
                vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
                vat.blendFactor = blendFactor;
                Debug.Log($"[{layerIndex}][VAT] next >=1, prev={vat.PrevAnimationIndex}, current={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
                return;
            }
            
            // 현재 애니메이션만 있는데 안에서 blend되고 있을 때 ...
            if (currentClipInfo.Length == 2)
            {
                var prev = currentClipInfo[0];
                var current = currentClipInfo[1];
                
                vat.PrevAnimationIndex = GetAnimationIndexByClip(prev.clip);
                vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
                vat.blendFactor = current.weight;
                Debug.Log($"[{layerIndex}][VAT] current==2, prev={vat.PrevAnimationIndex}, current={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
                return;
            }

            if (currentClipInfo.Length == 1)
            {
                var current = currentClipInfo[0];
                vat.PrevAnimationIndex = -1;
                vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
                vat.blendFactor = 1f;
                Debug.Log($"[{layerIndex}][VAT] current==1, prev={vat.PrevAnimationIndex}, current={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
                return;
            }
            

            Debug.LogWarning($"VATStateMachineBehaviour layer {layerIndex} has more than three current clips !!!");
            
        }
    }
}