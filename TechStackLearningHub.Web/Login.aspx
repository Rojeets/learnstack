<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="TechStackLearningHub.Web.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-6 mx-auto">
            <div class="card">
                <div class="card-body">
                    <h2 class="card-title">Log in</h2>
                    <div class="mb-3">
                        <label class="form-label" for="txtUsername">Username</label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label" for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <asp:Label ID="lblError" runat="server" CssClass="text-danger" Visible="false" />
                    <asp:Button ID="btnLogin" runat="server" Text="Log in" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>