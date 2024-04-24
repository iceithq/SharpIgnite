<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Forms.aspx.cs" Inherits="Forms" %>
<%@ Import Namespace="SharpIgnite" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <h3>Profile information</h3>

    <p>Username<br />
        <%= FormHelper.Input("username") %>
    </p>
    <p>Password<br />
        <%= FormHelper.Password("password") %>
    </p>
    <p>Email<br />
        <%= FormHelper.Email("email") %>
    </p>
    <p>Gender<br />
        <% var genders = SharpIgnite.Array.New("0", "Male")
                  .Add("1", "Female")
                  .Add("3", "Other"); %>
        <%= FormHelper.DropDown("gender", genders) %>
    </p>
    <p>Marital status<br />
        <%= FormHelper.Label("Male", "male") %>
        <%= FormHelper.Radio("maritalStatus", "0", true, "id='male'") %>
        <%= FormHelper.Label("Female", "female") %>
        <%= FormHelper.Radio("maritalStatus", "1", false, "id='female'") %>
    </p>
    <p>
        <%= FormHelper.CheckBox("terms", "1", true) %>
        I accept to the terms and conditions.
    </p>
    <p>
        <%= FormHelper.Submit("submit", "Update user information") %>
    </p>

</asp:Content>

