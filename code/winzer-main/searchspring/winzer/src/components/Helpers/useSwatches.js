import { useMemo } from 'preact/hooks'

export const useSwatches = ({
    variantColors,
    variantImages,
    swatchImages,
    fallbackImage,
}) => {
    return useMemo(() => {
        if (!variantColors || !swatchImages || !variantImages) {
            return []
        } else {
            let swatchList = new Set();
            let swatches = JSON.parse(swatchImages)
            let images = JSON.parse(variantImages)

            variantColors.forEach(function (color, index) {
                let swatch = new Object();

                swatch.color = color.replaceAll('  ', ' ');

                swatch.fallback_img = fallbackImage;

                images.forEach(function (img, index) {
                    if (img.color == swatch.color) {
                        swatch.thumbnail = img.img;
                    }
                })

                if (!swatch.thumbnail) {
                    swatch.thumbnail = fallbackImage
                }

                swatches.forEach(function (img, index) {
                    let splitImg = img.split('/');
                    let swatchMatcher = splitImg[splitImg.length-1];

                    if (swatchMatcher.split('.')[0] === (swatch.color.replaceAll(' ', '-').toLowerCase())) {
                        swatch.swatch_image = img
                    }
                })

                if (!swatch.swatch_image) {
                    swatch.swatch_image = fallbackImage
                }

                swatchList.add(swatch);
            });

            return swatchList;
        }
    }, [
        variantColors,
        variantImages,
        swatchImages,
        fallbackImage,
    ])
}
