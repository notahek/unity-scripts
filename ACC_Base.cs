using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ACC
{
    public abstract class ACC_Base : MonoBehaviour
    {
        [Header("Parameters")]
        public bool InitiateOnStart;
        public UnityEvent OnModuleInitiated;
        public bool InitializationState = false;

        /// <summary>
        /// You must Invoke this event --> OnModuleInitiated. It lets other scripts know if the module is Initiated or Not.
        /// </summary>
        public abstract void InitializeModule();

        private void Awake()
        {
            OnModuleInitiated.AddListener(() =>
            {
                InitializationState = true;
            });
        }

        protected void Start()
        {
            if (InitiateOnStart) InitializeModule();
        }
    }

    public static class AHEKUtility
    {
        #region Conversion Functions

        public static IEnumerator LerpFloatOverTime(float initial, float final, float duration, System.Action<float> onValueChanged)
        {
            float timer = 0;
            float output = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                output = Mathf.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpFloatOverTime(float initial, float final, float duration, System.Action<float> onValueChanged, float deltatimeMultiplier)
        {
            float timer = 0;
            float output = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime * deltatimeMultiplier;
                output = Mathf.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpColorOverTime(Color initial, Color final, float duration, System.Action<Color> onValueChanged)
        {
            float timer = 0;
            Color output;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                output = Color.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpColorOverTime(Color initial, Color final, float duration, System.Action<Color> onValueChanged, float deltatimeMultiplier)
        {
            float timer = 0;
            Color output;

            while (timer < duration)
            {
                timer += Time.deltaTime * deltatimeMultiplier;
                output = Color.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpVector2OverTime(Vector2 initial, Vector2 final, float duration, System.Action<Vector2> onValueChanged)
        {
            float timer = 0;
            Vector2 output;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                output = Vector2.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpVector2OverTime(Vector2 initial, Vector2 final, float duration, System.Action<Vector2> onValueChanged, float deltatimeMultiplier)
        {
            float timer = 0;
            Vector2 output;

            while (timer < duration)
            {
                timer += Time.deltaTime * deltatimeMultiplier;
                output = Vector2.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpVector3OverTime(Vector3 initial, Vector3 final, float duration, System.Action<Vector3> onValueChanged)
        {
            float timer = 0;
            Vector3 output;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                output = Vector3.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        public static IEnumerator LerpVector3OverTime(Vector3 initial, Vector3 final, float duration, System.Action<Vector3> onValueChanged, float deltatimeMultiplier)
        {
            float timer = 0;
            Vector3 output;

            while (timer < duration)
            {
                timer += Time.deltaTime * deltatimeMultiplier;
                output = Vector3.Lerp(initial, final, timer / duration);
                onValueChanged?.Invoke(output);

                yield return null;
            }
        }

        #endregion

        public static bool ContainsElementInArray<T>(T[] array, T element)
        {
            foreach(var item in array)
            {
                if(item.Equals(element)) return true;
            }

            return false;
        }
    }
}