import { Component, OnDestroy, OnInit } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject, Observable } from "rxjs";
import { map } from "rxjs/operators";
import { AuthorizeService } from "../../api-authorization/authorize.service";
import { NewMessage } from "../interfaces/newmessage.iterface";

/**
 * Chatroom component
 */
@Component({
  selector: "app-chatroom-component",
  template: `
    <div *ngIf="!joined">
      <strong>Create a group</strong>
      <div class="form-group row mb-2">
        <label class="col-form-label col-md-3">Group name</label>
        <div class="col-md-9">
          <input type="text" class="form-control" name="groupName" [(ngModel)]="groupName" />
        </div>
      </div>
      <div class="form-group row">
        <div class="col-md-9 offset-3">
          <button type="button" class="btn btn-primary" (click)="join()">
            Enter
          </button>
        </div>
      </div>
    </div>

    <div *ngIf="joined">
      <div id="chat">
        <div *ngFor="let message of (conversationSubject | async)">
          <div><strong>{{message.date.toLocaleTimeString()}} - {{message.userName}}:</strong> {{message.message}}</div>
        </div>
      </div>
      <input class="form-control mb-1" type="text" [(ngModel)]="messageToSend" name="messageToSend" />
      <button class="btn btn-primary me-2" (click)="sendMessage()">Send</button>
      <button class="btn btn-secondary" (click)="leave()">Leave</button>
    </div>
  `
})
export class ChatroomComponent implements OnInit {
  private userName = "";
  public conversationSubject = new BehaviorSubject<NewMessage[]>([{
    date: new Date(),
    message: "Bienvenido",
    userName: "Sistema"
  }]);
  public userNameObservable?: Observable<string | null | undefined>;
  public groupName = "";
  public messageToSend = "";
  public joined = false;

  private connection: HubConnection;

  constructor(private authorizeService: AuthorizeService) {
    let apiBaseUrl = "https://localhost:7234";
    this.connection = new HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/chatroom`)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on("NewUser", message => this.newUser(message));
    this.connection.on("NewMessage", message => this.newMessage(message));
    this.connection.on("LeftUser", message => this.leftUser(message));
  }

  public ngOnInit(): void {
    this.userNameObservable = this.authorizeService.getUser().pipe(map(u => u && u.name));
    this.userNameObservable.subscribe(newUserName => {
      this.userName = newUserName?.toString() ?? "";
    })
    this.connection.start()
      .then(_ => {
        console.log("Connection Started");
      }).catch(error => {
        return console.error(error);
      });
  }

  public join(): void {
    this.connection.invoke("JoinGroup", this.groupName, this.userName)
      .then(_ => {
        this.joined = true;
      });
  }

  public sendMessage(): void {
    const newMessage: NewMessage = {
      date: new Date(),
      message: this.messageToSend,
      userName: this.userName,
      groupName: this.groupName
    };

    this.connection.invoke("SendMessage", newMessage)
      .then(_ => {
        this.messageToSend = ""
      });
  }

  public leave(): void {
    this.connection.invoke("LeaveGroup", this.groupName, this.userName)
      .then(_ => this.joined = false);
  }

  private addMessageToConversation(message: NewMessage): void {
    let originalConv = this.conversationSubject.getValue();
    originalConv.push(message);
    let newConv = originalConv.slice(Math.max(originalConv.length - 50, 0));
    this.conversationSubject.next(newConv);
  }

  private newUser(message: string): void {
    this.addMessageToConversation({
      date: new Date(),
      userName: "Sistema",
      message: message
    });
  }

  private newMessage(message: NewMessage): void {
    let newDate = new Date(message.date);
    message.date = newDate;
    this.addMessageToConversation(message);
  }

  private leftUser(message: string): void {
    console.log(message);
    this.addMessageToConversation({
      date: new Date(),
      userName: "Sistema",
      message: message
    });
  }
}
