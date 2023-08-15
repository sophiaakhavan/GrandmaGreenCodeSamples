using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GrandmaGreen.Collections;

namespace GrandmaGreen.Entities
{
    public class ShopkeeperController : MonoBehaviour, IEmotionAgent
    {
        public Animator animator;
        public CharacterId id;
        public PlayerController playerController;

        // Start is called before the first frame update
        void Start()
        {
            EventManager.instance.EVENT_CHA_CHANGE_EMO += ChangeEmotion;
            EventManager.instance.EVENT_CHA_CHANGE_EMO_INTIME += ChangeEmotionInTime;
        }

        private void OnDisable() {
            EventManager.instance.EVENT_CHA_CHANGE_EMO -= ChangeEmotion;
            EventManager.instance.EVENT_CHA_CHANGE_EMO_INTIME -= ChangeEmotionInTime;
        }
        
        public void FaceToPlayer()
        {
            // facing to grandma
            Vector3 playerPos = playerController.GetEntityPos();
            Vector3 tempScale = this.transform.localScale;
            if (playerPos.x < transform.position.x) {
                if (tempScale.x < 0) {
                    tempScale.x *= -1;
                    this.transform.localScale = tempScale;
                }
            } else {
                if (tempScale.x > 0) {
                    tempScale.x *= -1;
                    this.transform.localScale = tempScale;
                }
            }
        }

        #region Emotion service
        public void ChangeEmotion(ushort CharID, ushort EmoID)
        {
            if (CharID == (ushort)id)
            {
                animator.SetInteger("EXPRESSION", EmoID);
            }
        }

        public void ChangeEmotionInTime(ushort CharID, ushort EmoID, float time)
        {
            if (CharID == (ushort)id)
            {
                StartCoroutine(ChangeEmotionInTimeCoroutine(EmoID, time));
            }
        }

        IEnumerator ChangeEmotionInTimeCoroutine(ushort EmoID, float time)
        {
            animator.SetInteger("EXPRESSION", EmoID);
            yield return new WaitForSeconds(time);
            animator.SetInteger("EXPRESSION", 0);
        }
        #endregion
    }
}
