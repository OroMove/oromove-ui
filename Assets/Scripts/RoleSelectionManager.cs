using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoleSelectionManager : MonoBehaviour
{
    public static string selectedRole; // Store user role

    public void SelectRole(string role)
    {
        selectedRole = role;
        SceneManager.LoadScene("SignUpPage"); // Redirect to Signup
    }
}
