import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { IMaskInput } from "react-imask";
import Aside from "../../core/Aside";
import Header from "../../core/Header";
import { makePrivateRequest } from "../../core/script";
import { Hash } from "../../core/types"

type AnalysisTable = {
    mergedArray: MergedItem[]
    dic: DicHash
}

type DicHash = {
    [key: string]: string[]
}

type PredictionResult = {
    insurerId: number;
    predicted: number;
}

type InsurerRating = {
    insurerId: number;
    insurerName: string;
    rating: string;
}

type MergedItem = {
    id: number;
    name: string;
    values: number[][]
}

const Analysis = () => {
    const { register, control, getValues, setValue } = useForm()

    const [table, setTable] = useState<AnalysisTable>()
    const [predictions, setPredictions] = useState<PredictionResult[]>()
    const [ratings, setRatings] = useState<InsurerRating[]>()
    const [hasError, setHasError] = useState(false)

    useEffect(() => {
        makePrivateRequest({ url: "/analysis/table" })
            .then(r => setTable(r.data))

            setValue('useCompanyRanting', false)
    }, [])

    const handlePrediction = () => {
        let rating = getValues('rating')
        let useCompanyRating = getValues('useCompanyRating')

        if(rating === undefined){
            return;
        }

        makePrivateRequest({url: '/analysis/predict', params: { rating, useCompanyRating }})
        .then(x => setPredictions(x.data))
    }

    const handleRatings = () => {
        let cnpj = getValues('cnpj')

        setHasError(false)

        makePrivateRequest({url: '/analysis/ratings', params: { cnpj }})
        .then(x => setRatings(x.data))
        .catch(() => setHasError(true))
    }

    return <section className="flex w-full h-full">
        <Aside />
        <div className="w-full bg-neutral-100">
            <Header />
            <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar">
            <div>
                <div className="text-center">
                    <h1 className="font-bold text-neutral-900 text-5xl my-4">Matriz de Risco</h1>
                    <p className="text-neutral-400 text-2xl font-medium">Utilize a matriz abaixo para prever os possíveis resultados para as taxas de um CNPJ. <br /> Essa é uma análise preditória e seus resultados não devem ser considerados verdade absoluta.</p>
                </div>
            </div>
            <div className="m-2">
                <table className="table-auto border-spacing-px border-separate text-sm w-full">
                    <thead>
                        <tr>
                            <th className="border border-slate-600">Rating</th>
                            {
                                table && table.mergedArray.map(x => (
                                    <th className="border border-slate-600">{x.name}</th>
                                ))
                            }
                        </tr>
                    </thead>
                    <tbody>
                        {
                            table && table.mergedArray[0].values.map((x, index) => (
                                <tr>
                                    <th className="border border-slate-600">{String.fromCharCode(index + 65)}</th>
                                    {
                                        table.mergedArray.map(y => (
                                            <td className="border border-slate-600 text-center hover:bg-slate-300 hover:cursor-pointer">{y.values[index][0].toFixed(2)} a {y.values[index][1].toFixed(2)}</td>
                                        ))
                                    }
                                </tr>
                            ))
                        }
                    </tbody>
                </table>
            </div>
            <div className="flex justify-around">
                <div className="m-2">
                    <table className="table-fixed border-spacing-px border-separate text-sm">
                        <thead>
                            <tr>
                                <th className="border border-slate-600">Rating</th>
                                <th className="border border-slate-600">Rating Seguradoras</th>
                            </tr>
                        </thead>
                        <tbody>
                            {table && Object.keys(table.dic).map(x => (
                                <tr>
                                    <th className="border border-slate-600">{x}</th>
                                    <th className="border border-slate-600 text-left hover:bg-slate-300 hover:cursor-pointer">{table.dic[x].join(", ")}</th>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
                <div className="mt-4">
                    <div className="flex flex-col form-input-field">
                        <label className="text-neutral-900 text-base font-semibold ml-0.5">Buscar ratings disponíveis através do cnpj</label>
                        <Controller
                            name='cnpj'
                            control={control}
                            render={({ field }) => (<IMaskInput mask="00.000.000/0000-00" unmask={true} value={field.value} onAccept={value => {field.onChange(value); setRatings(undefined)}} onComplete={() => handleRatings()}></IMaskInput>)}
                        />
                        <div className="flex flex-wrap">
                            {ratings && ratings.map(x => (
                                <div className="mx-2 py-2 px-4 shadow-md no-underline rounded-full bg-violet-600 text-white font-sans font-semibold text-sm">{x.insurerName} : {x.rating}</div>
                            ))}
                        </div>
                    </div>
                </div>
                <div className="mt-4 mr-2 flex flex-col">
                    <div className="flex flex-col form-input-field">
                        <label className="text-neutral-900 text-base font-semibold ml-0.5">Prever valor de acordo com o rating</label>
                        <Controller
                            name='rating'
                            control={control}
                            render={({ field }) => (<IMaskInput mask={"aa"} unmask={true} value={field.value} onAccept={field.onChange}></IMaskInput>)}
                        />
                    </div>
                    <div className="my-2">
                        <div className="flex items-center">
                            <label className="checkbox-control">
                                <input type={"checkbox"} {...register('useCompanyRating')}></input>
                                <span>Usar rating da seguradora</span>
                            </label>
                        </div>
                    </div>
                    <button className="auth-button rounded-xl p-1 text-white h-10" onClick={() => handlePrediction()}>Prever</button>
                </div>
            </div>
            <h6 className="m-2 text-neutral-400 text-2xl font-medium">Os resultados da predição aparecerão abaixo</h6>
            <div className="m-2">
                <table className="table-auto border-spacing-px border-separate text-sm w-full">
                    <thead>
                        <tr>
                            {predictions && predictions.map(x => (
                                <th className="border border-slate-600">{table?.mergedArray.find(y => y.id === x.insurerId)?.name}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            {predictions && predictions.map(x => (
                                <th className="border border-slate-600">{x.predicted.toFixed(2)}</th>
                            ))}
                        </tr>
                    </tbody>
                </table>
            </div>
            </div>
        </div>
    </section>

}

export default Analysis;