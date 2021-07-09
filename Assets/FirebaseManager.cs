using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;

    public static FirebaseManager Instance
    {
        get => instance;
        private set => instance = value;
    }

    private FirebaseFirestore db;
    private FirebaseUser user;
    
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        
        DontDestroyOnLoad(this);
        
        FirebaseApp app;
        
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
               
                app = Firebase.FirebaseApp.DefaultInstance;
    
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                
                Debug.Log("Firebase ready !");
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }


    public void AuthDebug()
    {
        // Connexion à firebase en anonyme

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
        
            user = task.Result;
            
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
            
        });
    }

    public void ConnectFirestoreDebug()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    
    public void AddNewPhotoDebug()
    {
        AddNewPhoto("imgstr", "thbstr");
    }
    
    public void AddNewPhoto(string image, string thumbnail)
    {
        CollectionReference photoRef = db.Collection("bubbles-photos");
        
        Dictionary<string, object> bubblephotoDoc = new Dictionary<string, object>
        {
            { "photo", image }
        };
        
        photoRef.AddAsync(bubblephotoDoc)
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("added photo to firestore, setting up the thumbnail to add...");
                AddThumbnail(thumbnail, task.Result.Id);
            });
    }

    private void AddThumbnail(string thumbnail, string photoid)
    {
        CollectionReference thumbnailRef = db.Collection("bubbles-thumbnails");

        Dictionary<string, object> thumbnailDoc = new Dictionary<string, object>
        {
            {"thumbnail", thumbnail},
            {"photoid", photoid}
        };
        thumbnailRef.AddAsync(thumbnailDoc).ContinueWithOnMainThread(task => {Debug.Log("added thumbnail to firestore");});
    }

    private void OnApplicationQuit()
    {
        db = null;
        user.Dispose();
    }

    private void OnDestroy()
    {
        db = null;
        Firebase.FirebaseApp.DefaultInstance.Dispose();

        user.Dispose();
    }
}
