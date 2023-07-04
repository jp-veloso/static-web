import { currencyDotted, formatCurrency } from "../../../../core/script";
import { ProposalItemType } from "../../../../core/types";
import _warn from "../../../../assets/information-circle.svg"

type Props = {
    item: ProposalItemType
}

const ProposalItem = ({ item }: Props) => {
    const keys = Object.keys(item.warns)

    const findBackgroundColor = (intensity: number) => {
        let color = 'white'

        if(item.status === 'FAILED'){
            color = intensity === 1 ? '#fecaca' : '#e11d48'
        }

        if(item.status === 'SUCCESS' && keys.length > 0){
            color = intensity === 1 ? '#fef3c7' : '#d97706'
        }

        return color;
    }

    return <div className="w-[240px] m-2 relative border border-transparent">
        <div className='left-[75px] absolute flex items-center justify-center rounded-full w-20 h-20 shadow-md bg-white overflow-hidden'>
            {item.insurer.picture ? <img src={item.insurer.picture} /> : <p className='text-xs font-bold text-center'>{item.insurer.name}</p>}
        </div>
        <div className="shadow-md mt-10 rounded-xl">
            <div className={`pt-14 pb-4 px-4 rounded-t-xl bg-white grid gap-1 grid-cols-2 grid-flow-row`}>
                <label className="text-sm text-neutral-600">
                    <span>Taxa base</span>
                    <p className="font-semibold">{String(item.rate.toFixed(2)).replace(".", ",")}</p>
                </label>
                <label className="text-sm text-neutral-600">
                    <span>Limite</span>
                    <p className="font-semibold">{currencyDotted(item.balance).trim()}</p>
                </label>
                <label className="text-sm text-neutral-600">
                    <span>Prêmio</span>
                    <p className="font-semibold">{formatCurrency(item.bounty)}</p>
                </label>
                <label className="text-sm text-neutral-600">
                    <span>Comissão</span>
                    <p className="font-semibold">{(item.commission * 100).toFixed(2)}%</p>
                </label>
            </div>

            <div className="h-20 overflow-y-scroll overflow-x-hidden scrollbar text-sm rounded-b-xl p-4" style={{backgroundColor: findBackgroundColor(1)}}>
                <div className="flex">
                    <img className='mr-1' src={_warn}></img>
                    <span className="text-sm font-semibold">Avisos</span>
                </div>
                {keys.map(x => (
                    <p key={x}><span className="font-semibold" style={{color: findBackgroundColor(2)}}>{x}:</span> {item.warns[x]}</p>
                ))}
            </div>
        </div>
    </div>

}

export default ProposalItem;