<%@ Page Title="Reset your password" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="TechStackLearningHub.Web.Account.ResetPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-6 mx-auto">
            <div class="card">
                <div class="card-body">
                    <h1 class="h4 card-title">Reset your password</h1>
                    <%-- Exactly one of the two panels below is made visible in
                         Page_Load, from whether the token is present. A visitor
                         with no token never sees the form at all, so there is
                         nothing on the page for them to fill in and nothing to
                         post back. --%>
                    <asp:Panel ID="pnlInvalidLink" runat="server" Visible="false">
                        <%-- One message, identical to the one AuthBLL uses when
                             a token turns out to be spent or expired. A second
                             wording here would be a second answer to the same
                             question, which is how a reset form becomes a probe.
                             role="alert" so the refusal is announced instead of
                             the visitor being left looking for a form that is
                             not there. --%>
                        <asp:Label ID="lblInvalidLink" runat="server" CssClass="text-danger d-block" role="alert"
                            Text="This reset link is invalid or has expired. Please request a new one." />
                        <a class="btn btn-link ps-0" href="<%= ResolveUrl("~/Account/ForgotPassword.aspx") %>">Request a new reset link</a>
                    </asp:Panel>
                    <asp:Panel ID="pnlResetForm" runat="server" Visible="false">
                        <div class="mb-3">
                            <asp:Label runat="server" AssociatedControlID="txtNewPassword" CssClass="form-label" Text="New password" />
                            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Password is required." CssClass="text-danger" Display="Dynamic" />
                        </div>
                        <div class="mb-3">
                            <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label" Text="Confirm password" />
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Please confirm your password." CssClass="text-danger" Display="Dynamic" />
                            <asp:CompareValidator runat="server" ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtNewPassword" Operator="Equal"
                                ErrorMessage="Passwords do not match." CssClass="text-danger" Display="Dynamic" />
                        </div>
                        <%-- role="alert" so a rejected reset is announced rather
                             than appearing silently below the fields. The form
                             stays rendered underneath: a spent or expired token
                             is the visitor's to fix, and hiding the form would
                             make a mistyped password look like a dead link. --%>
                        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block" Visible="false" role="alert" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset password" CssClass="btn btn-primary" OnClick="btnReset_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
