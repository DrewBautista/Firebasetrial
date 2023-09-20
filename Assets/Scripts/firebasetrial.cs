using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseController : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseUser user;
    DatabaseReference databaseReference;

    [SerializeField] string child = "data0";
    [SerializeField] Data data;
    [SerializeField] Data loadedData;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        auth.SignInAnonymouslyAsync();
    }

    private void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        print("Auth State Changed.");
        if (auth.CurrentUser == null || auth.CurrentUser != user)
        {
            user = auth.CurrentUser;

            if (user == null) GetUser();
            else HandleDatabase();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            string jsonData = JsonUtility.ToJson(data);

            print($"Create data at child: {jsonData}");

            CreateData(child, jsonData);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            string jsonData = JsonUtility.ToJson(data);

            print($"Create data at root: {jsonData}");

            CreateData(jsonData);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            print("Clear data at child");

            ClearData(child);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            print("Clear all data.");

            ClearData();
        }
    }

    void GetUser()
    {
        print("Logging in Anonymously.");
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to log in anonymously.");
                return;
            }

            Debug.Log("Logged in anonymously.");
        });
    }

    void HandleDatabase()
    {
        print("Handling Database");

        databaseReference = FirebaseDatabase.GetInstance("https://trial2-1345f-default-rtdb.asia-southeast1.firebasedatabase.app/").RootReference;

        databaseReference.ValueChanged += (object sender, ValueChangedEventArgs eventArgs) =>
        {
            string data = eventArgs.Snapshot.GetRawJsonValue();

            print($"Database value has changed: {data}");

            try
            {
                loadedData = JsonUtility.FromJson<Data>(data);
            }
            catch (System.Exception)
            {
                // Do nothing or handle the exception according to your needs
            }
        };
    }

    void CreateData(string child, string jsonData)
    {
        if (databaseReference != null)
            databaseReference.Child(child).SetRawJsonValueAsync(jsonData);
    }

    void CreateData(string jsonData)
    {
        if (databaseReference != null)
            databaseReference.SetRawJsonValueAsync(jsonData);
    }

    void ClearData(string child)
    {
        if (databaseReference != null)
            databaseReference.Child(child).RemoveValueAsync();
    }

    void ClearData()
    {
        if (databaseReference != null)
            databaseReference.RemoveValueAsync();
    }
}

[System.Serializable]
class Data
{
    public int index;
    public string employeeName;
    public EmergencyContact[] emergencyContacts;
}

[System.Serializable]
class EmergencyContact
{
    public string contactName;
    public string contactPhoneNumber;
}