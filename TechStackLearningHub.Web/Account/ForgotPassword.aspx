<%@ Page Title="Forgot your password" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="TechStackLearningHub.Web.Account.ForgotPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-6 mx-auto">
            <div class="card">
                <div class="card-body">
                    <h1 class="h4 card-title">Reset your password</h1>
                    <p class="text-muted">Enter the email address on your account and we'll send you a link to choose a new password.</p>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" autocomplete="email" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." CssClass="text-danger" Display="Dynamic" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                            ErrorMessage="Enter a valid email address." CssClass="text-danger" Display="Dynamic" />
                    </div>
                    <%-- role="alert" so the confirmation is announced rather than
                         appearing silently under the field. Note the CssClass is
                         REPLACED (not appended to) by the code-behind, so the
                         visible result carries the colour and not the d-block,
                         exactly as Register.aspx does it. --%>
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-success d-block" Visible="false" role="alert" />
                    <asp:Button ID="btnSendLink" runat="server" Text="Send reset link" CssClass="btn btn-primary" OnClick="btnSendLink_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
