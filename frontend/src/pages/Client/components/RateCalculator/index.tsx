import { useState } from "react";
import { Controller, FieldValues, useForm } from "react-hook-form";
import { IMaskInput } from "react-imask";

type Props = {
    cancel: () => void;
}

type CalculatorForm = {
    bounty: string;
    insuredAmount: string;
    period: string;
}

const RateCalculator = ({ cancel }: Props) => {
    const { control, formState, handleSubmit } = useForm<CalculatorForm>()
    const [result, setResult] = useState(0)

    const getRate = (values: FieldValues) => {
        let bounty = Number(values["bounty"].replaceAll(".", "").replaceAll(",", "."))
        let insuredAmount = Number(values["insuredAmount"].replaceAll(".", "").replaceAll(",", "."))
        let rate = (bounty * 365) / (insuredAmount * Number(values["period"])) * 100
        setResult(rate)
    }

    return <form className="absolute z-10 w-screen h-screen bg-neutral-400 bg-opacity-40 flex justify-center items-center" onSubmit={handleSubmit(x => {getRate(x)})}>
        <div className="bg-white rounded-lg shadow p-5 relative m-2 w-96">
            <h4 className="text-2xl text-neutral-900 font-bold mt-2 mb-1">Descobrir a taxa</h4>
            <p className="text-neutral-500 text-sm mb-4">Insira as informações e descubra a taxa vendida</p>
            <div>
                <label className="form-input-field">
                    <span className="text-neutral-500 text-base ml-0.5">Prêmio (R$)</span>
                    <Controller
                        name={"bounty"}
                        control={control}
                        rules={{ required: true }}
                        render={({ field }) => (
                            <IMaskInput
                                mask={Number}
                                value={String(field.value)}
                                scale={2}
                                thousandsSeparator="."
                                padFractionalZeros={false}
                                normalizeZeros={true}
                                radix={","}
                                mapToRadix={["."]}
                                onComplete={field.onChange}
                            />
                        )}
                    />
                </label>
                <label className="form-input-field">
                    <span className="text-neutral-500 text-base ml-0.5">Vigência (dias)</span>
                    <Controller
                        name={"period"}
                        control={control}
                        rules={{ required: true }}
                        render={({ field }) => (
                            <IMaskInput
                                mask={Number}
                                value={String(field.value)}
                                onComplete={field.onChange}
                            />
                        )}
                    />
                </label>
                <label className="form-input-field">
                    <span className="text-neutral-500 text-base ml-0.5">Importância Segurada (R$)</span>
                    <Controller
                        name={"insuredAmount"}
                        control={control}
                        rules={{ required: true }}
                        render={({ field }) => (
                            <IMaskInput
                                mask={Number}
                                value={String(field.value)}
                                scale={2}
                                thousandsSeparator="."
                                padFractionalZeros={false}
                                normalizeZeros={true}
                                radix={","}
                                mapToRadix={["."]}
                                onComplete={field.onChange}
                            />
                        )}
                    />
                </label>
            </div>
            <button className="auth-button rounded-xl p-4 text-white w-full cursor-pointer mt-6">Descobrir taxa</button>
            <button onClick={() => cancel()} type="button" className="absolute top-1 right-1 text-neutral-900 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm p-1.5 inline-flex items-center dark:hover:bg-violet-300 dark:hover:text-white">
                <svg aria-hidden="true" className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd"></path></svg>
            </button>
            {result != 0 && (<div className="w-full flex items-center justify-center">
            
                <div className="bg-gradient-to-r from-violet-600 to-violet-700 text-white text-sm rounded-md p-4 mt-4 w-fit font-semibold">TAXA VENDIDA: <span className="font-semibold">{result.toFixed(2).replace(".",",")}%</span></div>
            </div>)}
        </div>
    </form>
}

export default RateCalculator;