import { Component, OnDestroy, OnInit } from "@angular/core";
import { ProcessErrorArgs, ServiceBusClient, ServiceBusError, ServiceBusReceivedMessage } from "@azure/service-bus";
import * as signalR from "@microsoft/signalr";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject, Observable } from "rxjs";
import { map } from "rxjs/operators";
import { AuthorizeService } from "../../api-authorization/authorize.service";

@Component({
  selector: "app-chatroom-component",
  templateUrl: "./chatroom.component.html"
})
export class ChatroomComponent implements OnInit, OnDestroy {
  private userName = "";
  private connectionString = "Endpoint=sb://josbityresponsechat.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=meVqi4gMC7ATkmfSAKH90nCYZqf1sIA0Nu1pGZDG2ok=";
  private queueName = "jobsityresponse";
  private sbClient: ServiceBusClient = new ServiceBusClient(this.connectionString);
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
    //let apiBaseUrl = "https://jobsitychatsignalr.service.signalr.net";
    this.connection = new HubConnectionBuilder()
      //.withUrl("https://localhost:7234/jobsitychatsignalr")
      //.withUrl(`${apiBaseUrl}/api`, {
      //  skipNegotiation: true,
      //  transport: signalR.HttpTransportType.WebSockets
      //})
      .withUrl(`${apiBaseUrl}/chatroom`)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on("NewUser", message => this.newUser(message));
    this.connection.on("NewMessage", message => this.newMessage(message));
    this.connection.on("LeftUser", message => this.leftUser(message));
  }

  public ngOnDestroy(): void {
    this.sbClient.close();
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

    this.sbClient = new ServiceBusClient(this.connectionString);
    let receiver = this.sbClient.createReceiver(this.queueName);
    let receiverSubscription = receiver.subscribe({
      processMessage: async (brokeredMessage: ServiceBusReceivedMessage) => {
        console.log(`Received message: ${brokeredMessage.body}`);
        const newMessage: NewMessage = {
          date: new Date(),
          message: brokeredMessage.body.toString(),
          userName: "Jobsity Stock Assistant",
          groupName: this.groupName
        };
        this.connection.invoke("SendMessage", newMessage);
      },
      processError: async (args: ProcessErrorArgs) => {
        console.log(`Error from source ${args.errorSource} occurred: `, args.error);
        if ((args.error as ServiceBusError).code == "UnauthorizedAccess") {
          receiverSubscription.close();
        }
      }
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
    let newConv = originalConv.slice(Math.max(originalConv.length - 10, 0));
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

interface NewMessage {
  date: Date;
  userName: string;
  message: string;
  groupName?: string;
}
