import { Component } from "@angular/core";

@Component({
  selector: "app-nav-menu",
  template: `
    <header>
      <nav
        class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3"
      >
        <div class="container">
          <a class="navbar-brand" [routerLink]="['/']">Jobsity Chatroom</a>
          <button
            class="navbar-toggler"
            type="button"
            data-toggle="collapse"
            data-target=".navbar-collapse"
            aria-label="Toggle navigation"
            [attr.aria-expanded]="isExpanded"
            (click)="toggle()"
          >
            <span class="navbar-toggler-icon"></span>
          </button>
          <div
            class="navbar-collapse collapse d-sm-inline-flex justify-content-end"
            [ngClass]="{ show: isExpanded }"
          >
            <ul class="navbar-nav flex-grow">
              <li
                class="nav-item"
                [routerLinkActive]="['link-active']"
                [routerLinkActiveOptions]="{ exact: true }"
              >
                <a class="nav-link text-dark" [routerLink]="['/']">Home</a>
              </li>
              <li class="nav-item" [routerLinkActive]="['link-active']">
                <a class="nav-link text-dark" [routerLink]="['/chatroom']"
                  >Chatroom</a
                >
              </li>
            </ul>
            <app-login-menu></app-login-menu>
          </div>
        </div>
      </nav>
    </header>
  `,
  styles: [`
    a.navbar-brand {
      white-space: normal;
      text-align: center;
      word-break: break-all;
    }

    html {
      font-size: 14px;
    }
    @media (min-width: 768px) {
      html {
        font-size: 16px;
      }
    }

    .box-shadow {
      box-shadow: 0 .25rem .75rem rgba(0, 0, 0, .05);
    }
  `]
})
export class NavMenuComponent {
  public isExpanded = false;

  public collapse(): void {
    this.isExpanded = false;
  }

  public toggle(): void {
    this.isExpanded = !this.isExpanded;
  }
}
