import { Component, createSignal, For, Show } from 'solid-js';

import logo from './logo.svg';
import styles from './App.module.css';
import SignalRConnection from './services/websocket';
import EntryComponent from './components/EntryComponent';

type TradeDetail = {
    m: boolean,
    p: number,
    q: number,
}

const App: Component = () => {

    const signalrConnection = new SignalRConnection();

    signalrConnection.connect();

    signalrConnection.connectionOn("aggrtrade", (data: TradeDetail) => {
        if (data.p * data.q > 1000)
            setList(prev => [data, ...prev]);
    });

    const [list, setList] = createSignal<TradeDetail[]>([]);

    // setInterval(()=> 
    // {
    //     let trade: TradeDetail = {price: Math.random()*10, quantity: 20, source: "green"};
    //     setList( prev => [trade, ...prev]);
    // },1000);

    // setInterval(()=> 
    // {
    //     let trade: TradeDetail = {price: Math.random()*10, quantity: 20, source: "red"};
    //     setList( prev => [trade, ...prev]);
    // },100);

    return (
        <div class="container mx-auto place-content-center">
            <For each={list()} fallback={<div> Loading...</div>}>
                {(item) => <EntryComponent price={item.p} color={item.m} quantity={item.q}></EntryComponent>}
            </For>
        </div>
    );
};

export default App;
