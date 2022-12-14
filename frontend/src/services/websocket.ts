import * as signalR from "@microsoft/signalr";


interface SignalRConnectionInterface {
    signalRHubConnection: signalR.HubConnection;
    connectionOn(methodName: string, func: () => void): void;
    connect(): void;
}

class SignalRConnection implements SignalRConnectionInterface {

    signalRHubConnection: signalR.HubConnection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5000/aggrtrade", { withCredentials: false })
        .build();

    connect() {
        this.signalRHubConnection.start().catch((err) => console.log(err));
    }

    connectionOn(methodName: string, func: (data: any) => void) {
        this.signalRHubConnection.on(methodName, func);
    };
}

export default SignalRConnection;