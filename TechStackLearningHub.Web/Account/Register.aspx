<%@ Page Title="Register" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="TechStackLearningHub.Web.Account.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-6 mx-auto">
            <div class="card">
                <div class="card-body">
                    <h1 class="h4 card-title">Create an account</h1>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" autocomplete="username" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" autocomplete="email" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." CssClass="text-danger" Display="Dynamic" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                            ErrorMessage="Enter a valid email address." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label" Text="Confirm password" />
                        <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                        <asp:CompareValidator runat="server" ControlToValidate="txtConfirmPassword"
                            ControlToCompare="txtPassword" Operator="Equal"
                            ErrorMessage="Passwords do not match." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <%-- role="alert" so a rejected registration is announced rather than appearing silently. --%>
                    <asp:Label ID="lblError" runat="server" CssClass="text-danger d-block" Visible="false" role="alert" />
                    <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>