﻿using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    public abstract class ActionResult : IActionResult
    {
        private protected string controllerName;
        private protected string viewName;

        private Object result;
        public Object Result { get => result; private protected set => result = value; }

        private Transform parent;
        public Transform Parent { get => parent; private protected set => parent = value; }

        private ViewContainer viewContainer;
        public ViewContainer ViewContainer { get => viewContainer; private protected set => viewContainer = value; }

        private AsyncOperationHandle handle;
        public AsyncOperationHandle Handle { get => handle; }

        private GameObject instantiatedObject;
        internal GameObject InstantiatedObject { get => instantiatedObject; }

        public delegate void ResultInstantiated(ActionResult view);
        public event ResultInstantiated OnResultInstantiated;

        private string routeUrl;
        internal string RouteUrl { get => routeUrl; set => routeUrl = value; }

        public virtual async Task ExecuteResultAsync()
        {
            string address = GetAddress();

            handle = await AddressableLoader.LoadAssetAsync<GameObject>(address);

            Result = (GameObject)handle.Result;

            if (Result == null)
                throw new System.ArgumentNullException("Result", $"Couldn't find view at location - " +
                    $"/Resources/{address}");

            instantiatedObject = Instantiate();
            OnResultInstantiated?.Invoke(this); // invoke related event
        }

        /// <summary>
        /// Instantiate Result as GameObject
        /// </summary>
        /// <param name="setParent">Set instantiated GameObject to available parent, if available.</param>
        /// <returns>Instantiated GameObject</returns>
        private GameObject Instantiate(bool setParent = true)
        {
            GameObject go = null;

            if (setParent && parent)
                go = Object.Instantiate((GameObject)Result, parent);
            else
                go = Object.Instantiate((GameObject)Result);
            
            return go;
        }

        internal bool Destroy()
        {
            if (instantiatedObject == null)
                return false;
            try 
            {
                UnityEngine.Object.Destroy(instantiatedObject);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Could not Destroy Action Result - " + e.Message);
                return false;
            }
        }

        internal void ReleaseReference()
        {
            if (!handle.IsValid())
                return;

            // release handle
            Addressables.Release(handle);
        }

        internal string GetAddress()
        {
            if(string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(viewName))
                return string.Empty;

            string address = $"{controllerName}/{viewName}";
            return address;
        }
    }
}