import { Component, createSignal, For, Show } from 'solid-js';

import logo from './logo.svg';
import styles from './App.module.css';
import SignalRConnection from './services/websocket';
import EntryComponent from './components/EntryComponent';

type TradeDetail = {
    direction: boolean,
    price: number,
    quantity: number,
    source: string,
}

const App: Component = () => {

    const signalrConnection = new SignalRConnection();

    signalrConnection.connect();

    signalrConnection.connectionOn("aggrtrade", (data: TradeDetail) => {
        if (data.price * data.quantity > 1000)
            setList(prev => [data, ...prev]);
    });

    const [list, setList] = createSignal<TradeDetail[]>([]);

    return (
        <div class="container mx-auto place-content-center">
            <For each={list()} fallback={<div> Loading...</div>}>
                {(item) => <EntryComponent source={item.source} price={item.price} color={item.direction} quantity={item.quantity}></EntryComponent>}
            </For>
        </div>
    );
};

export default App;
