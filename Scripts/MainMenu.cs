using System;
using Godot;

public partial class MainMenu : BoxContainer
{
    [Export] LineEdit ipInput;
    [Export] LineEdit portInput;
    [Export] Button joinButton;
    [Export] Button hostButton;
    [Export] TwoCarController twoCarController;

    public override void _Ready()
    {
        hostButton.ButtonDown += OnHostButton;
        joinButton.ButtonDown += OnJoinButton;
    }

    private void OnJoinButton()
    {
        if (!int.TryParse(portInput.Text, out int port))
            throw new InvalidCastException("Port is not a valid integer");
        twoCarController.StartClient(ipInput.Text, port);
        Hide();
    }

    private void OnHostButton()
    {
        twoCarController.StartHost();
        Hide();
    }
}
