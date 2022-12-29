import { ConsoleLogger } from "@microsoft/signalr/dist/esm/Utils";
import { Component } from "solid-js";
import styles from "./EntryComponent.module.css";
import chroma from "chroma-js";

const EntryComponent: Component<{ color: boolean, price: number, quantity: number, source: string }> = (props) => {
    if (props.color)
        return (
            <div class="mx-auto text-center flex flex-auto" style={getColor(props.price * props.quantity, true)}>
                <div class="basis-1/3">{props.source}</div>
                <div class="basis-1/3">{props.price}</div>
                <div class="basis-1/3">{props.quantity * props.price}</div>
            </div>
        );
    else
        return (
            <div class="mx-auto text-center flex flex-auto" style={getColor(props.price * props.quantity, false)}>
                <div class="basis-1/3">{props.source}</div>
                <div class="basis-1/3">{props.price}</div>
                <div class="basis-1/3">{props.quantity * props.price}</div>
            </div>
        );
};

function getColor(tradeSize: number, tradeSide: boolean) {
    let minThreshold = 1000;
    let maxThreshold = 10000;
    let diff = maxThreshold - minThreshold;
    let buyColorScale = chroma.scale(['white', 'red']).padding([0.4,0]);
    let sellColorScale = chroma.scale(['green', 'Lime']).padding([0.2,0]);

    if (tradeSide) {
        return `background-color: rgb(${sellColorScale((tradeSize - minThreshold) / diff).rgb()})`;
    }
    else {

        return `background-color: rgb(${buyColorScale((tradeSize - minThreshold) / diff).rgb()})`;
    }
}


export default EntryComponent;