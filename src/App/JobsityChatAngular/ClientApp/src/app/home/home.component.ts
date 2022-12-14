import { Component } from "@angular/core";

/**
 * Welcome page
 * */
@Component({
  selector: "app-home",
  template: `
    <h1>Jobsity - Chatroom!</h1>
    <p>Welcome to your new Chatroom application, you can join the chat here:</p>
    <ul>
      <li [routerLinkActive]="['link-active']">
        <a class="nav-link text-dark" [routerLink]="['/chatroom']">Chatroom (by Groups)</a>
      </li>
    </ul>
    <p>Possible commands:</p>
    <ul>
      <li><strong>/stock=stock_code</strong>. For example, if you ask the current Apple stocks values, you can send: <em>/stock=AAPL.US</em>.</li>
    </ul>
  ` 
})
export class HomeComponent {
}
